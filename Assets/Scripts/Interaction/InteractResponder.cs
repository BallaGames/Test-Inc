using Balla.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractResponder : BallaNetScript
{
    public Triggerable[] triggerables;
    public Dictionary<Triggerable, bool> triggers;
    public bool reached;

    public bool TriggersOK => triggers.All(x => x.Value == true);

    public override void OnNetworkSpawn()
    {
        triggers = new();
        Debug.Log("Spawned interact Responder");
        //for every triggerable
        for (int i = 0; i < triggerables.Length; i++)
        {
            Debug.Log($"Subscribed {gameObject.name} to events on this object:", triggerables[i]);
            triggerables[i].TriggerSet += TriggerReceived;
            triggerables[i].TriggerReset += TriggerReset;
            triggers.Add(triggerables[i], false);
        }

    }

    public virtual void ConditionReached()
    {
        Debug.Log("condition reached");
        reached = true;
    }
    public virtual void ConditionReset()
    {
        Debug.Log("condition reset");
        reached = false;
    }

    public virtual void TriggerReset(Triggerable triggerable)
    {
        if (triggerables.Contains(triggerable))
        {
            triggers[triggerable] = false;
            Debug.Log("Triggers contains this triggerable, resett");
        }
        if (reached)
        {
            ConditionReset();
        }
        Debug.Log($"TriggersOK on Reset : {TriggersOK}");
    }

    public virtual void TriggerReceived(Triggerable triggerable)
    {
        if (triggerables.Contains(triggerable))
        {
            triggers[triggerable] = true;
        }
        if (TriggersOK)
        {
            ConditionReached();
        }
        Debug.Log($"TriggersOK on Receive: {TriggersOK}");
    }
}
