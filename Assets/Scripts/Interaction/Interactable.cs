using Balla.Core;
using Balla.Gameplay.Player;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;


/// <summary>
/// Base class for interactable objects. Do not conflate this with grabbable/carriable objects.<br></br>
/// Interactable objects are those that the player must presss a key/button to interact with. 
/// Objects that are classed as "world interactions" such as objects on pressure pads should NOT use the Interactable system directly,
/// though those objects can also have interactable elements to them.<para></para>
/// Interactable objects can facilitate deeper gameplay through systems the player can directly influence or interact with.
/// Examples of such objects could include buttons and doors.<para></para>
/// Interactable components cannot be on multiple object and require a rigidbody. That rigidbody should be set as Kinematic unless the object should be moved.
/// Since the Interaction system relies on raycasts to detect them, 
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Rigidbody))]
public class Interactable : BallaNetScript
{
    public NetworkVariable<bool> isInteractable = new();
    public NetworkVariable<bool> Interacting = new();

    public UnityEvent u_OnInteractStart, u_OnInteractEnd, u_OnInteract;
    public Action e_OnInteractStart, e_OnInteractCanel, e_OnInteract;
    public Rigidbody rb;

    public PlayerInteractor currentInteractor;

    public bool holdInteract;
    public float interactTime;
    protected float currentTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

    }

    /// <summary>
    /// Called by the player that is starting an interaction on this object.<br></br>
    /// Sends the interact message to everyone.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="data"></param>
    [Rpc(SendTo.Everyone)]
    public virtual void SetInteract_RPC(bool state, RpcParams data = default)
    {
        if (IsServer)
            Interacting.Value = state;

        if (state)
            InteractStart();
        else
            InteractEnd();

        if (!holdInteract && state)
        {
            OnInteract();
        }
    }
    /// <summary>
    /// Called when <see cref="holdInteract"/> is true and the player starts interacting with the object.
    /// </summary>
    public virtual void InteractStart()
    {
        u_OnInteractStart?.Invoke();
        e_OnInteractStart?.Invoke();
    }
    /// <summary>
    /// Called when <see cref="holdInteract"/> is true and the player stops interacting with the object BEFORE interaction completes.
    /// </summary>
    public virtual void InteractEnd()
    {
        u_OnInteractEnd?.Invoke();
        e_OnInteractCanel?.Invoke();
    }
    /// <summary>
    /// Called when either:<br></br>
    /// > <see cref="holdInteract"/> is false and we interacted with this object<br></br>
    /// > <see cref="holdInteract"/> is true and we interacted with this object until interaction was completed.
    /// </summary>
    public virtual void OnInteract()
    {
        u_OnInteract?.Invoke();
        e_OnInteract?.Invoke();
    }
}
