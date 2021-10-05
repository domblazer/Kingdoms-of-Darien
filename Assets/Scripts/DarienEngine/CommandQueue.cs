using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DarienEngine
{
    public class CommandQueue : Queue<CommandQueueItem>
    {
        public CommandQueueItem Last { get; private set; }
        new public void Enqueue(CommandQueueItem item)
        {
            base.Enqueue(item);
            Last = item;
            item.PlaceCommandSticker();
        }

        new public CommandQueueItem Dequeue()
        {
            CommandQueueItem dq = base.Dequeue();
            dq.RemoveCommandSticker();
            return dq;
        }
    }

    public class CommandQueueItem
    {
        public CommandTypes commandType;
        public Vector3 commandPoint;
        // type is Conjurer?: 
        public ConjurerArgs conjurerArgs;
        // type is Attack?: 
        public AttackInfo attackInfo;
        // type is Patrol?:
        // @Note: entire patrol route is kept within one command item since patrol never dequeues
        public PatrolRoute patrolRoute;

        public GameObject commandSticker;

        public void PlaceCommandSticker()
        {
            if (CommandMappings.StickerMap.TryGetValue(commandType, out GameObject sticker))
                commandSticker = GameManager.Instance.InstantiateHelper(sticker, new Vector3(commandPoint.x, commandPoint.y + 0.1f, commandPoint.z));
        }

        public void RemoveCommandSticker()
        {
            if (commandSticker != null)
                GameManager.Instance.DestroyHelper(commandSticker);
        }

        public override string ToString()
        {
            string str = "CommandType: " + commandType + "\n" + "CommandPoint: " + commandPoint;
            str += "ConjurerArgs?: " + conjurerArgs;
            str += "AttackInfo?: " + conjurerArgs;
            str += "PatrolRoute?: " + patrolRoute;
            return str;
        }
    }

    public class ConjurerArgs
    {
        public Button menuButton;
        public GameObject prefab;
        public Vector3 buildSpot;
        public int buildQueueCount = 0;
        public override string ToString()
        {
            string str = "\nMenu Button: " + menuButton + "\n";
            str += "Prefab: " + prefab + "\n";
            str += "Build Queue Count: " + buildQueueCount + "\n";
            return str;
        }
    }

    public class AttackInfo
    {
        public GameObject attackTarget;
        public bool targetBaseUnit { get { return attackTarget.GetComponent<RTSUnit>(); } }
        public override string ToString()
        {
            return "AttackTarget: " + attackTarget.name;
        }
    }

    public class PatrolPoint
    {
        public Vector3 point;
        public GameObject sticker;
        public override string ToString()
        {
            return point.ToString();
        }
    }

    public class PatrolRoute
    {
        public List<PatrolPoint> patrolPoints;

        public override string ToString()
        {
            return string.Join<PatrolPoint>(", ", patrolPoints.ToArray());
        }
    }

    public static class CommandMappings
    {
        public static Dictionary<CommandTypes, CursorManager.CursorType> CursorMap = new Dictionary<CommandTypes, CursorManager.CursorType>
        {
            [CommandTypes.Move] = CursorManager.CursorType.Move,
            [CommandTypes.Attack] = CursorManager.CursorType.Attack,
            [CommandTypes.Conjure] = CursorManager.CursorType.Repair,
            [CommandTypes.Guard] = CursorManager.CursorType.Guard,
            [CommandTypes.Patrol] = CursorManager.CursorType.Patrol,
            // @TODO
        };

        public static Dictionary<CommandTypes, GameObject> StickerMap = new Dictionary<CommandTypes, GameObject>
        {
            [CommandTypes.Move] = GameManager.Instance.moveCommandSticker,

            [CommandTypes.Guard] = GameManager.Instance.guardCommandSticker,
            [CommandTypes.Patrol] = GameManager.Instance.patrolCommandSticker
            // @TODO
        };
    }
}