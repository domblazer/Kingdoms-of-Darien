using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DarienEngine.Clustering
{
    public class DescriptUnit
    {
        GameObject unit;
        public bool isOutsidePrimaryCluster = false;
    }

    public class UnitClusterMoveInfo
    {
        public List<DescriptUnit> descriptUnits = new List<DescriptUnit>();
        public bool primaryClusterExists = false;
        public float standardDeviation = 0.0f;
        public Vector3 smartCenter;
        public int primaryClusterCount;
        public int outlierCount;
    }

    public static class Clusters
    {
        public class MoveGroupInfo
        {
            public float radius;
            public Dictionary<RTSUnit, Vector3> unitMovePositions;
        }
        public static MoveGroupInfo MoveGroup(List<RTSUnit> selectedUnits, Vector3 groundHitPoint, Vector3 skyHitPoint, bool addToMoveQueue = false, bool doAttackMove = false)
        {
            // UnitClusterMoveInfo clusterMoveInfo = CalculateSmartCenter(selectedUnits);

            float radius = selectedUnits[0].offset.x;
            float deg360 = 0;
            float counter = 0;
            int countOffset = selectedUnits.Count;
            List<Vector3> positions = new List<Vector3>();
            // @TODO: small groups of units seem to need to be tightened up
            // @TODO: need to sort units to move to the closest proposed point to them. Need to create virtual move point cluster,
            // then sort, then move to appropriate points
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                RTSUnit unit = selectedUnits[i];

                // Distance around the circle 
                // @TODO: tighten up the area (maybe not a circle) if only a handful of units
                float radians = 2 * (Mathf.PI / countOffset) * i * ((unit.offset.x * 0.65f) * 2);
                float deg = Mathf.Rad2Deg * radians;
                // Debug.Log("deg: " + deg);
                deg360 = deg - counter;
                if (deg360 >= 360)
                {
                    radius += unit.offset.x;
                    deg360 = 0;
                    counter += 360;
                    countOffset += i;
                    // Debug.Log("circle passed one rev, up radius to " + radius);
                }

                // Get the vector direction 
                float vertical = Mathf.Sin(radians);
                float horizontal = Mathf.Cos(radians);
                // Debug.Log("vertical: " + vertical + " horizontal: " + horizontal + " i: " + i);
                Vector3 spawnDir = new Vector3(horizontal, 0, vertical);

                // Calculate moveTo
                Vector3 moveTo = groundHitPoint + spawnDir * radius;
                positions.Add(moveTo);

                /* Vector3 offset = (unit.transform.position - clusterMoveInfo.smartCenter);
                Vector3 moveTo = hitPoint + offset;
                // Debug.Log("clusterMoveInfo.standardDeviation " + clusterMoveInfo.standardDeviation);
                // If unit is outside the normal distribution, consider it outside primary cluster and must adjust move to collapse in
                if (offset.sqrMagnitude > clusterMoveInfo.standardDeviation)
                {
                    Debug.Log("Mathf.Sqrt(clusterMoveInfo.standardDeviation) " + Mathf.Sqrt(clusterMoveInfo.standardDeviation));
                    // @TODO: need to use offset direction with stdDev magnitude
                    // moveTo = hitPoint + (offset.normalized * Mathf.Sqrt(clusterMoveInfo.standardDeviation));
                    moveTo = hitPoint + (offset.normalized * Mathf.Sqrt(clusterMoveInfo.standardDeviation) / 2);
                    // Debug.Log("I " + unit.name + " am outside the primary cluster, moving to " + moveTo);
                } */

                // @TODO: also need to make sure moveTo points don't overlap or get too close to eachother
                // @TODO // if (noPrimaryCluster) // everyone collapse in around click point naively?

                // if (unit && unit.isKinematic)
                //    unit.SetMove(moveTo, addToMoveQueue);
            }

            MoveGroupInfo moveGroupInfo = new MoveGroupInfo { radius = radius, unitMovePositions = new Dictionary<RTSUnit, Vector3>() };
            foreach (RTSUnit unit in selectedUnits)
            {
                Vector3 moveTo = FindClosestPosition(unit, positions);
                // Once this moveTo has been picked, it should not be considered again
                positions.Remove(moveTo);
                moveGroupInfo.unitMovePositions.Add(unit, moveTo);
                // @TODO: need to do TestPoint() on these positions so no units get told to move to invalid locations
                // @TODO: SetMove for flying units
                if (unit && unit.isKinematic)
                {
                    if (unit.canFly)
                    {
                        unit._FlyingUnit.lastCorrespondingGroundPoint = groundHitPoint;
                    }
                    // @TODO: adjustedMoveTo for flying units
                    unit.SetMove(moveTo, addToMoveQueue, doAttackMove);
                }
            }

            return moveGroupInfo;
        }

        private static Vector3 FindClosestPosition(RTSUnit unit, List<Vector3> positions)
        {
            Vector3 closest = Vector3.zero;
            float distance = Mathf.Infinity;
            foreach (Vector3 pos in positions)
            {
                Vector3 dist = unit.transform.position - pos;
                float curDistance = dist.sqrMagnitude;
                if (curDistance < distance)
                {
                    closest = pos;
                    distance = curDistance;
                }
            }
            return closest;
        }

        public static UnitClusterMoveInfo CalculateSmartCenter(List<RTSUnit> group)
        {
            // Calculate "SmartCenter" of the selected units
            //   1. Calculate average position of selectedUnits
            //   2. Remove any units that aren’t within 1 standard deviation of that average
            //   3. Recalculate the average position from that subset of the group
            List<DescriptUnit> descriptUnits = new List<DescriptUnit>();
            Vector3 mean = Vector3.zero;
            foreach (RTSUnit unit in group)
                mean += unit.transform.position; // @TODO see if I can use median
            mean = mean / group.Count;

            // Take the sum of the squared lengths of differences with the average
            float sumOfSquares = group.Sum(d => ((d.transform.position - mean).sqrMagnitude));
            // @Note: normally you take the square root of (sum/count), but that's expensive and unnecessary for this task
            float stdd = (sumOfSquares) / (group.Count() - 1);

            // Check if there is a significant cluster of units by summing their radii and comparing against standard deviation
            float radiiSum = group.Sum(d => d.GetComponent<RTSUnit>().offset.x);
            bool primaryClusterExists = radiiSum > stdd;

            // Now calculate the adjusted mean
            Vector3 adjustedMean = Vector3.zero;
            int adjustedCount = 0;
            foreach (RTSUnit unit in group)
            {
                // If unit is within one standard deviation of the initial average, include it in the adjusted set
                if ((unit.transform.position - mean).sqrMagnitude <= stdd)
                {
                    adjustedCount++;
                    adjustedMean += unit.transform.position;
                }
            }
            adjustedMean = adjustedMean / adjustedCount;
            // Debug.Log("Adjusted average: " + adjustedMean);

            return new UnitClusterMoveInfo
            {
                descriptUnits = descriptUnits,
                smartCenter = adjustedMean,
                standardDeviation = stdd,
                primaryClusterExists = primaryClusterExists,
                primaryClusterCount = adjustedCount,
                outlierCount = group.Count - adjustedCount
            };
        }

        public static List<ClusterInfo> Cluster(List<RTSUnit> units, int numClusters)
        {
            // @TODO: Normalize data (remove outliers?)

            // Init clusters
            List<ClusterInfo> clusters = new List<ClusterInfo>();
            for (int i = 0; i < numClusters; i++)
                clusters.Add(new ClusterInfo());
            // Assign each unit to a cluster at random
            foreach (RTSUnit unit in units)
                clusters[Random.Range(0, numClusters)].units.Add(unit);

            bool changed = true; // was there a change in at least one cluster assignment?
            bool success = true; // were all means able to be computed? (no zero-count clusters)

            int maxCount = units.Count * 10; // sanity check
            int ct = 0;
            while (changed == true && success == true && ct < maxCount)
            {
                ++ct; // k-means typically converges very quickly
                success = UpdateMeans(clusters); // compute new cluster means if possible. no effect if fail
                changed = UpdateClustering(units, clusters); // (re)assign tuples to clusters. no effect if fail
            }
            return clusters;
        }

        private static bool UpdateMeans(List<ClusterInfo> clusters)
        {
            // If any cluster has no points, bad clustering no change to means
            if (clusters.Any(x => x.units.Count == 0))
                return false;
            foreach (ClusterInfo cluster in clusters)
                cluster.Mean();
            return true;
        }

        private static bool UpdateClustering(List<RTSUnit> units, List<ClusterInfo> clusters)
        {
            // New clusters to represent proposed changes
            List<ClusterInfo> newClusters = new List<ClusterInfo>();
            for (int i = 0; i < clusters.Count; i++)
                newClusters.Add(new ClusterInfo());

            bool changed = false;
            float[] distances = new float[clusters.Count];
            // Check all points against each cluster mean
            for (int i = 0; i < units.Count; i++)
            {
                // Distances indices correspond to cluster indices
                for (int j = 0; i < clusters.Count; i++)
                    distances[j] = SqrDistance(units[i].transform.position, clusters[j].mean);
                // Find index of nearest mean 
                int newClusterIndex = MinIndex(distances);
                // If the new index is not the current cluster index, it is a change
                if (newClusterIndex != i)
                {
                    changed = true;
                    newClusters[newClusterIndex].units.Add(units[i]);
                }
            }
            // If nothing was changed or a new cluster has no units, bad result, no changes
            if (!changed || newClusters.Any(x => x.units.Count == 0))
                return false;
            // Make the change
            clusters = newClusters;
            return true;
        }

        public static float SqrDistance(Vector3 pointA, Vector3 pointB)
        {
            return (pointA - pointB).sqrMagnitude;
        }

        private static int MinIndex(float[] distances)
        {
            // index of smallest value in array
            int indexOfMin = 0;
            double smallDist = distances[0];
            for (int k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        }
    }

    public class ClusterInfo
    {
        public List<RTSUnit> units;
        public Vector3 mean;

        public ClusterInfo()
        {
            units = new List<RTSUnit>();
            mean = Vector3.zero;
        }

        public Vector3 Mean()
        {
            Vector3 sum = Vector3.zero;
            foreach (RTSUnit unit in units)
                sum += unit.transform.position;
            mean = (sum / units.Count);
            return mean;
        }
    }
}