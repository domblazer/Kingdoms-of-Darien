using UnityEngine;
using System.Collections.Generic;
using DarienEngine.Clustering;
using System.Linq;

namespace DarienEngine.AI
{
    public enum AIProfileTypes
    {
        Balanced, Turtle
    }

    public class AIPlayerContext
    {
        public GameObject holder;
        public AIPlayer player;
        public InventoryAI inventory;
        public TeamNumbers team;
    }

    [System.Serializable]
    public class BuildUnit
    {
        public UnitCategories unitCategory;
        public GameObject intangiblePrefab;
        public override string ToString()
        {
            return intangiblePrefab ? intangiblePrefab.GetComponent<IntangibleUnitAI>().finalUnit.unitName : "NULL";
        }
    }

    /// <summary>
    /// Class <c>Army</c> manages the behavior of an AI army. 
    /// </summary>
    public class Army
    {
        public List<RTSUnit> units;
        private int originalUnitCount;
        public bool ordersIssued = false;
        public bool doneFormingUp = false;
        private Vector3 formUpPoint;
        private float formUpTimeLimit = 30.0f;
        private float formUpTimeRemaining = 30.0f;
        public bool inRetreat { get { return (float)units.Count / (float)originalUnitCount < 0.2f; } }
        public bool retreatOrdersIssued = false;
        private float avgVelocity;
        private string statusText = "";
        private Clusters.MoveGroupInfo moveGroupInfo;
        private RTSUnit attackTarget;

        public class PlayerSnapshot
        {
            public PlayerNumbers playerNumber;
            public Vector3 startPosition;
            public InventoryBase inventory;

            public PlayerSnapshot(PlayerNumbers playerNum)
            {
                playerNumber = playerNum;
            }
        }
        public PlayerSnapshot playerSnapshot;
        public PlayerSnapshot enemySnapshot;

        public Army(List<RTSUnit> armyUnits)
        {
            foreach (BaseUnitAI unit in armyUnits)
                unit._Army = this;
            units = armyUnits;
            originalUnitCount = units.Count;
        }

        public override string ToString()
        {
            string str = "\n";
            str += "Unit Count: " + units.Count + "\n";
            str += "Status: " + statusText;
            return str;
        }

        public void HandleUpdate()
        {
            // @TODO: army groups need to move together and not split if there is some obstacle that causes some to go one path and the rest another

            if (ordersIssued && !doneFormingUp && !inRetreat)
            {
                statusText = "Forming up";
                // Debug.Log("Army is forming up...");
                formUpTimeRemaining -= Time.deltaTime;
                // Army is done forming up when all units have arrived at their move points or time limit reached
                doneFormingUp = units.All(u => u.commandQueue.IsEmpty()) | formUpTimeRemaining < 0;
                if (formUpTimeRemaining < 0)
                    formUpTimeRemaining = formUpTimeLimit;

                if (doneFormingUp)
                {
                    Debug.Log("Army is formed up, now launching attack.");
                    
                    // @TODO: this pruning is naiive and maybe unessesary 
                    // Take the distance of each unit and discard any outside certain radius
                    /* List<RTSUnit> newUnits = new List<RTSUnit>();
                    foreach (RTSUnit unit in units)
                    {
                        float sqrDist = (formUpPoint - unit.transform.position).sqrMagnitude;
                        if (sqrDist < Mathf.Pow(moveGroupInfo.radius, 2))
                            newUnits.Add(unit);
                    }
                    originalUnitCount = newUnits.Count;
                    units = newUnits;
                    // Units booted from the army in this check go back to patrolling
                    RTSUnit[] exceptUnits = units.Except<RTSUnit>(newUnits, new RTSUnitComparer()).ToArray();
                    foreach (RTSUnit exUnit in exceptUnits)
                        exUnit.currentCommand = new CommandQueueItem { commandType = CommandTypes.Patrol, patrolRoute = null }; */

                    // @TODO: take another Army.PlayerSnapshot here? To accurately reflect enemy status when attack orders given

                    BeginAttack();
                }
            }
            else if (ordersIssued && doneFormingUp && !inRetreat)
            {
                // @TODO: units in an Army should keep trying to attack as long as they are not broken, so once an attack interrupt has
                // happened, they need to go back to picking another target with FindTarget()
                statusText = "Engaging enemy";
                // Debug.Log("Army is now engaging target...");

                foreach (RTSUnit unit in units)
                {
                    // If this unit has no attack target and no enemies nearby to attack, find new target
                    if (unit._AttackBehavior.attackTarget == null && unit._AttackBehavior.enemiesInSight.Count == 0)
                    {
                        // @TODO: probably shouldn't call FindTarget many times since it does K-means
                        // should probably only do k-means every so often and store the cluster info as a static var
                        RTSUnit target = FindTarget(unit.transform.position);
                        if (target)
                            unit._AttackBehavior.TryInterruptAttack(target.gameObject);
                    }
                }
            }
            else if (inRetreat)
            {
                statusText = "Retreating";
                Debug.Log("Army is broken, retreating");

                // Issue retreat orders once, not every frame
                if (!retreatOrdersIssued)
                {
                    IssueRetreat();
                    retreatOrdersIssued = true;
                }
            }
            // @TODO: Charge/Run routine?
        }

        public void UnitReturnToPatrol(object sender, CommandQueue.CommandQueueChangedEventArgs changeEvent)
        {
            if (changeEvent.changeType == "Dequeue")
            {
                CommandQueue queueRef = sender as CommandQueue;
                // tell this unit to go back to patrolling after its current command gets dequeued
                queueRef.Enqueue(new CommandQueueItem { commandType = CommandTypes.Patrol, patrolRoute = null });
            }
        }

        // When units die and are removed from main inventory they need to be removed from army too
        public void HandleUnitChange(object sender, InventoryBase.OnInventoryChangedEventArgs e)
        {
            if (!e.wasAdded)
                units.Remove(e.unitAffected);
        }

        public void PlayerConditions(PlayerNumbers playerNum, PlayerNumbers versusPlayerNum)
        {
            // Setup this player's snapshot
            playerSnapshot = SetupPlayerSnapshot(playerNum);
            enemySnapshot = SetupPlayerSnapshot(versusPlayerNum);
        }

        private PlayerSnapshot SetupPlayerSnapshot(PlayerNumbers pNum)
        {
            PlayerSnapshot pSnap = new PlayerSnapshot(pNum);
            if (pNum == PlayerNumbers.Player1)
            {
                pSnap.startPosition = GameManager.Instance.PlayerMain.player.playerStartPosition.position;
                pSnap.inventory = GameManager.Instance.PlayerMain.player.inventory;
            }
            else if (GameManager.Instance.AIPlayers.TryGetValue(pNum, out AIPlayerContext aiPlayer))
            {
                pSnap.startPosition = aiPlayer.player.playerStartPosition.position;
                pSnap.inventory = aiPlayer.player.inventory;
            }
            return pSnap;
        }

        public void FormUpAndAttack(Vector3 point)
        {
            formUpPoint = point;
            ordersIssued = true;
            Clusters.MoveGroup(units, formUpPoint, formUpPoint, false, true);
        }

        /* public Vector3 FormUp()
        {
            formUpPoint = FindFormUpLocation();

            // @Note: attackMove so units can still attack if engaged while forming up
            // @TODO: still problem though that these units will get interrupted and need to return to these army orders 
            // @TODO: point for skyFormUpPoint
            moveGroupInfo = Clusters.MoveGroup(units, formUpPoint, formUpPoint, false, true);
            return formUpPoint;
        } 

        // Find a "quarter" point between this player's start position and the enemy's, towards player's position
        public Vector3 FindFormUpLocation()
        {
            // To get "quarter" point, take midpoint of midpoint
            Vector3 midPoint = MidPoint(playerSnapshot.startPosition, enemySnapshot.startPosition);
            Vector3 qPoint = MidPoint(playerSnapshot.startPosition, midPoint);
            return qPoint;
        }

        private Vector3 MidPoint(Vector3 startPoint, Vector3 endPoint)
        {
            return startPoint - ((startPoint - endPoint) / 2);
        }*/

        public void BeginAttack()
        {
            attackTarget = FindTarget(formUpPoint);
            Debug.Log("Army attack target: " + attackTarget.gameObject.name);
            if (attackTarget != null)
            {
                float sumVelocity = 0.0f;
                // Tell all army units to attack, and get their average speed
                foreach (RTSUnit armyUnit in units)
                {
                    sumVelocity += armyUnit.maxSpeed;
                    // @Note: doing an attack ensures the enemy will follow a target
                    armyUnit._AttackBehavior.TryInterruptAttack(attackTarget.gameObject);
                }
                avgVelocity = sumVelocity / units.Count;
                // Normalize the move speed of this army based on average
                foreach (RTSUnit unit in units)
                    unit.SetSpeed(avgVelocity);

                ordersIssued = true;
            }
        }

        public void IssueRetreat()
        {
            Vector3 retreatPoint = formUpPoint;
            // Set units back to o.g. speeds and tell them to return to patrolling once they are back
            foreach (RTSUnit unit in units)
            {
                unit.ResetMaxSpeed();
                (unit as BaseUnitAI)._Army = null;
                unit.commandQueue.OnQueueChanged += UnitReturnToPatrol;
            }
            // @TODO: units should only need to get within range of the retreat point before dequeueing
            // @TODO: point for skyFormUpPoint
            if (units.Count > 0)
                moveGroupInfo = Clusters.MoveGroup(units, retreatPoint, retreatPoint, false);
        }

        public RTSUnit FindTarget(Vector3 fromOrigin)
        {
            RTSUnit targetUnit = null;
            // If enemy has enough units, use k-means clustering to find good target
            if (enemySnapshot.inventory.totalUnits.Count > 30)
            {
                // @TODO: problem with k-means cluster is have to specifiy num clusters upfront, when could be many
                List<ClusterInfo> clusters = Clusters.Cluster(enemySnapshot.inventory.totalUnits, 3);
                // Find the closest cluster
                ClusterInfo closestCluster = clusters[0];
                float closestClusterDistance = Clusters.SqrDistance(fromOrigin, clusters[0].mean);
                foreach (ClusterInfo cluster in clusters)
                {
                    if (Clusters.SqrDistance(fromOrigin, cluster.mean) < closestClusterDistance)
                    {
                        closestClusterDistance = Clusters.SqrDistance(fromOrigin, cluster.mean);
                        closestCluster = cluster;
                    }
                }
                // Now find the unit closest to that cluster's mean and do an attack move on him
                RTSUnit closestUnitInCluster = enemySnapshot.inventory.totalUnits[0];
                float closestUnitDistance = Clusters.SqrDistance(closestCluster.mean, enemySnapshot.inventory.totalUnits[0].transform.position);
                foreach (RTSUnit unit in enemySnapshot.inventory.totalUnits)
                {
                    if (Clusters.SqrDistance(closestCluster.mean, unit.transform.position) < closestUnitDistance)
                    {
                        closestUnitDistance = Clusters.SqrDistance(closestCluster.mean, unit.transform.position);
                        closestUnitInCluster = unit;
                    }
                }
                targetUnit = closestUnitInCluster;

            }
            // Fewer than enough units, just pick randomly from total
            else
                targetUnit = enemySnapshot.inventory.totalUnits[Random.Range(0, enemySnapshot.inventory.totalUnits.Count)];

            return targetUnit;
        }
    }

    public interface IUnitBuilderAI
    {
        void QueueBuild(GameObject intangiblePrefab);
    }
}