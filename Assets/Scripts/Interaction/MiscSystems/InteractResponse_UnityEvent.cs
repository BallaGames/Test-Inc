using UnityEngine;
using UnityEngine.Events;

public class InteractResponse_UnityEvent : InteractResponder
{
    public UnityEvent u_Reached, u_Reset;

    public override void ConditionReached()
    {
        base.ConditionReached();
        Debug.Log("invoked set events on interact response");
        u_Reached?.Invoke();
    }
    public override void ConditionReset()
    {
        base.ConditionReset();
        Debug.Log("invoked reset events on interact response");
        u_Reset?.Invoke();
    }
}
