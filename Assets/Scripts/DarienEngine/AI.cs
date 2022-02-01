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

    public class Army
    {
        public List<RTSUnit> units;
        private int originalUnitCount;
        public bool ordersIssued = false;
        public bool doneFormingUp = false;
        private Vector3 formUpPoint;
        public bool isBroken { get { return (float)units.Count / (float)originalUnitCount < 0.2f; } }

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

        public Army(List<RTSUnit> allValidUnits, int armySize)
        {
            units = new List<RTSUnit>();
            for (int i = 0; i < armySize; i++)
            {
                // Collect this army from a random selection of valid units
                int rand = Random.Range(0, allValidUnits.Count);
                units.Add(allValidUnits[rand]);
            }
            originalUnitCount = armySize;
        }

        public void HandleUpdate()
        {
            if (ordersIssued && !doneFormingUp && !isBroken)
            {
                Debug.Log("Army is forming up...");
                doneFormingUp = units.All(u => u.commandQueue.IsEmpty());

                if (doneFormingUp)
                {
                    // @TODO: take another Army.PlayerSnapshot here? To accurately reflect enemy status when attack orders given
                    Debug.Log("Army is formed up, now launching attack.");
                    BeginAttack();
                }

                // @TODO: ArmyObjectives { Attack, Scatter/Disperse, Retreat, Charge }
            }
            else if (isBroken)
            {
                Debug.Log("Army is broken, retreating");
                IssueRetreat();
                // @TODO: when units get back to retreat point, they go back to roaming
            }
        }

        // When units die they need to be removed from army too
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

        public void FormUp()
        {
            formUpPoint = FindFormUpLocation();
            Clusters.MoveGroup(units, formUpPoint, false);
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
        }

        public void BeginAttack()
        {
            RTSUnit attackTarget = FindTarget(formUpPoint);
            Debug.Log("Army attack target: " + attackTarget.gameObject.name);
            if (attackTarget != null)
                foreach (RTSUnit armyUnit in units)
                    armyUnit.TryAttack(attackTarget.gameObject);
        }

        public void IssueRetreat()
        {
            // @TODO: use startPosition as retreatPoint?
            Vector3 retreatPoint = playerSnapshot.startPosition;
            // @TODO: these units also need to be set to passive so they do not attack anymore
            Clusters.MoveGroup(units, retreatPoint, false);
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
                RTSUnit closestUnitToCluster = enemySnapshot.inventory.totalUnits[0];
                float closestUnitDistance = Clusters.SqrDistance(closestCluster.mean, enemySnapshot.inventory.totalUnits[0].transform.position);
                foreach (RTSUnit unit in enemySnapshot.inventory.totalUnits)
                {
                    if (Clusters.SqrDistance(closestCluster.mean, unit.transform.position) < closestUnitDistance)
                    {
                        closestUnitDistance = Clusters.SqrDistance(closestCluster.mean, unit.transform.position);
                        closestUnitToCluster = unit;
                    }
                }
                targetUnit = closestUnitToCluster;
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