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
        public static void MoveGroup(List<BaseUnit> selectedUnits, Vector3 hitPoint, bool addToMoveQueue = false, bool attackMove = false)
        {
            UnitClusterMoveInfo clusterMoveInfo = CalculateSmartCenter(selectedUnits);
            
            // System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            // st.Start();
            foreach (BaseUnit unit in selectedUnits)
            {
                Vector3 offset = (unit.transform.position - clusterMoveInfo.smartCenter);
                Vector3 moveTo = hitPoint + offset;

                // If unit is outside the normal distribution, consider it outside primary cluster and must adjust move to collapse in
                if (offset.sqrMagnitude > clusterMoveInfo.standardDeviation)
                {
                    // @TODO: need to use offset direction with stdDev magnitude
                    moveTo = hitPoint + (offset.normalized * 2); // new Vector3(hitPoint.x + clusterMoveInfo.standardDeviation, hitPoint.y, hitPoint.z + clusterMoveInfo.standardDeviation);
                    // Debug.Log("I " + unit.name + " am outside the primary cluster, moving to " + moveTo);
                }
                // @TODO: also need to make sure moveTo points don't overlap or get too close to eachother
                // @TODO // if (noPrimaryCluster) // everyone collapse in around click point naively?

                if (unit && unit.isKinematic)
                    unit.SetMove(moveTo, addToMoveQueue, attackMove);
            }
            // st.Stop();
            // Debug.Log(string.Format("{0} Selected units took {1} ms to complete", selectedUnits.Count, st.ElapsedMilliseconds));
        }

        public static UnitClusterMoveInfo CalculateSmartCenter(List<BaseUnit> group)
        {
            // Calculate "SmartCenter" of the selected units
            //   1. Calculate average position of selectedUnits
            //   2. Remove any units that aren’t within 1 standard deviation of that average
            //   3. Recalculate the average position from that subset of the group
            List<DescriptUnit> descriptUnits = new List<DescriptUnit>();
            Vector3 mean = Vector3.zero;
            List<Vector3> positions = new List<Vector3>();
            foreach (BaseUnit unit in group)
            {
                positions.Add(unit.transform.position);
                mean += unit.transform.position;
                // @TODO see if I can use median
            }
            mean = mean / group.Count;

            // Take the sum of the squared lengths of differences with the average
            float sumOfSquares = positions.Sum(d => ((d - mean).sqrMagnitude));
            // @Note: normally you take the square root of (sum/count), but that's expensive and unnecessary for this task
            float stdd = (sumOfSquares) / (positions.Count() - 1);

            // Check if there is a significant cluster of units by summing their radii and comparing against standard deviation
            float radiiSum = group.Sum(d => d.GetComponent<RTSUnit>().offset.x);
            bool primaryClusterExists = false;
            if (radiiSum > stdd)
            {
                Debug.Log("We have a primary cluster.");
                primaryClusterExists = true;
            }

            // Now calculate the adjusted mean
            Vector3 adjustedMean = Vector3.zero;
            int adjustedCount = 0;
            foreach (BaseUnit unit in group)
            {
                // If unit is within one standard deviation of the initial average, include it in the adjusted set
                if ((unit.transform.position - mean).sqrMagnitude <= stdd)
                {
                    adjustedCount++;
                    adjustedMean += unit.transform.position;
                }
                else
                {
                    // Debug.Log("This unit " + unit.name + " is not within 1 standard deviation of average position.");
                    // @TODO: this unit needs to be told to go to hitPoint + offset(with magnitude = stdd)
                }
            }
            adjustedMean = adjustedMean / adjustedCount;
            // Debug.Log("Adjusted average: " + adjustedMean);

            // If a primary cluster exists but at least 1 unit is more than 1 standard deviation away from mean
            // if (primaryClusterExists && adjustedCount < group.Count)

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
    }
}