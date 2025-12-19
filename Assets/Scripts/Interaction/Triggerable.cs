using Balla.Core;
using System;
using Unity.Netcode;
using UnityEngine;

public class Triggerable : BallaNetScript
{
    public NetworkVariable<bool> triggered = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool onlyTriggerOnce;

    public Action<Triggerable> TriggerSet, TriggerReset;

    public void SetTrigger()
    {
        SendTrigger_RPC();
    }
    [Rpc(SendTo.Everyone, DeferLocal = true)]
    public void SendTrigger_RPC()
    {
        Debug.Log("send trigger from object", gameObject);
        TriggerSet?.Invoke(this);
        PostTriggerSet();
        if (IsServer)
        {
            triggered.Value = true;
        }
    }
    [Rpc(SendTo.Everyone, DeferLocal = true)]
    public void ResetTrigger_RPC()
    {
        if (onlyTriggerOnce)
            return;
        Debug.Log("reset trigger from object", gameObject);
        TriggerReset?.Invoke(this);
        PostTriggerReset();
        if (IsServer)
        {
            triggered.Value = false;
        }
    }
    public void ResetTrigger()
    {
        ResetTrigger_RPC();
    }
    protected virtual void PostTriggerSet()
    {

    }
    protected virtual void PostTriggerReset()
    {

    }
}
