using Balla.Core;
using System;
using Unity.Netcode;
using UnityEngine;

public class Triggerable : BallaNetScript
{
    public NetworkVariable<bool> triggered = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool onlyTriggerOnce;

    public Action<Triggerable> TriggerSet, TriggerReset;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        triggered.OnValueChanged += TriggerValueChanged;
        TriggerValueChanged(false, triggered.Value);
    }

    protected virtual void TriggerValueChanged(bool previous, bool current)
    {
        if(!previous && current)
        {
            TriggerTrue();
            return;
        }
        if(previous && !current)
        {
            TriggerFalse();
            return;
        }
    }

    public void SetTrigger()
    {
        if (IsServer)
        {
            triggered.Value = true;
        }
    }
    public void TriggerTrue()
    {
        Debug.Log("send trigger from object", gameObject);
        TriggerSet?.Invoke(this);
        PostTriggerSet();
    }
    public void TriggerFalse()
    {
        if (onlyTriggerOnce)
            return;
        Debug.Log("reset trigger from object", gameObject);
        TriggerReset?.Invoke(this);
        PostTriggerReset();
    }
    public void ResetTrigger()
    {
        if (IsServer)
        {
            triggered.Value = false;
        }
    }
    protected virtual void PostTriggerSet()
    {

    }
    protected virtual void PostTriggerReset()
    {

    }
}
