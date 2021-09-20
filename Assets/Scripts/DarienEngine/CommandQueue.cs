using UnityEngine;

namespace DarienEngine
{
    public class CommandQueueItem
    {
        public CommandTypes commandType;
        public Vector3 commandPoint;
        // type is Conjurer?: 
        public PlayerConjurerArgs playerConjurerArgs;
        // type is Attack?: 
        public AttackInfo attackInfo;
    }

    public class AttackInfo
    {
        public GameObject attackTarget;
        public bool targetBaseUnit { get { return attackTarget.GetComponent<RTSUnit>(); } }
    }
}