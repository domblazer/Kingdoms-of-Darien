using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace DarienEngine
{
    public class CommandQueue : List<CommandQueueItem>
    {
        public class CommandQueueChangedEventArgs
        {
            public string changeType;
            public CommandQueueItem command;
        }
        public event EventHandler<CommandQueueChangedEventArgs> OnQueueChanged;
        public CommandQueueItem Last { get; private set; }
        public GameObject referrer;
        public bool isAI = false;

        public void Enqueue(CommandQueueItem item)
        {
            item.OnCommandItemChanged += ItemChanged;
            Add(item);
            Last = item;
            if (!isAI)
                item.PlaceCommandSticker();
            OnQueueChanged?.Invoke(this, new CommandQueueChangedEventArgs { changeType = "Enqueue", command = item });
        }

        public CommandQueueItem Dequeue()
        {
            CommandQueueItem dq = null;
            if (Count > 0)
            {
                dq = base[0];
                RemoveAt(0);
                if (!isAI)
                    dq.RemoveCommandSticker();
            }
            OnQueueChanged?.Invoke(this, new CommandQueueChangedEventArgs { changeType = "Dequeue", command = dq });
            return dq;
        }

        public CommandQueueItem Peek()
        {
            return base[0];
        }

        public void InsertFirst(CommandQueueItem item)
        {
            base.Insert(0, item);
            OnQueueChanged?.Invoke(this, new CommandQueueChangedEventArgs { changeType = "InsertFirst", command = item });
        }

        public void ItemChanged(object sender, CommandQueueItem.CommandItemChangedEventArgs changeEvent)
        {
            if (changeEvent.changeType == "stickerClicked")
            {
                changeEvent.command.RemoveCommandSticker();
                base.Remove(changeEvent.command);
            }
        }

        new public void Clear()
        {
            if (!isAI)
                foreach (CommandQueueItem cmd in base.ToArray())
                    cmd.RemoveCommandSticker();
            base.Clear();
            OnQueueChanged?.Invoke(this, new CommandQueueChangedEventArgs { changeType = "Clear", command = null });
        }

        public override string ToString()
        {
            string str = "\n";
            foreach (CommandQueueItem item in base.ToArray())
                str += "-----------------\n" + item + "\n-----------------";
            return str;
        }
    }

    public class CommandQueueItem
    {
        public class CommandItemChangedEventArgs
        {
            public CommandQueueItem command;
            public CommandTypes commandType;
            public string changeType = "";
        }
        public event EventHandler<CommandItemChangedEventArgs> OnCommandItemChanged;
        public CommandTypes commandType;
        public bool isAttackMove = false;
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
            {
                commandSticker = GameManager.Instance.InstantiateHelper(sticker, new Vector3(commandPoint.x, commandPoint.y + 0.1f, commandPoint.z));
                commandSticker.GetComponent<CommandSticker>().OnClick(HandlePointClicked);
                if (!InputManager.HoldingShift())
                    commandSticker.SetActive(false);
            }
        }

        public void HandlePointClicked()
        {
            OnCommandItemChanged?.Invoke(this, new CommandItemChangedEventArgs { commandType = commandType, changeType = "stickerClicked", command = this });
        }

        public void RemoveCommandSticker()
        {
            if (commandSticker != null)
                GameManager.Instance.DestroyHelper(commandSticker);
        }

        public override string ToString()
        {
            string str = "CommandType: " + commandType + "\n" + "CommandPoint: " + commandPoint + "\n";
            if (commandType == CommandTypes.Conjure)
                str += "ConjurerArgs?: " + conjurerArgs;
            else if (commandType == CommandTypes.Attack)
                str += "AttackInfo?: " + conjurerArgs;
            else if (commandType == CommandTypes.Patrol)
                str += "PatrolRoute?: " + patrolRoute;
            return str;
        }
    }

    public class ConjurerArgs
    {
        // menuButton is the actual UI button used by a Player to instantiate a build
        public Button menuButton;
        // clickHandler registers the click events on the buttons and queues a build on the UI button click
        public ClickableObject clickHandler;
        // prefab is the GhostUnit prefab
        public GameObject prefab;
        // unitCategory is only used by UnitBuilderAI/BuilderAI
        public UnitCategories unitCategory;
        // buildSpot is only used by BuilderAI
        public Vector3 buildSpot;
        // buildQueueCount is used by Factory to set units queued count over menu button
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