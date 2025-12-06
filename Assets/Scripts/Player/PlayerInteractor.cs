using Balla;
using Balla.Core;
using Balla.Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This script enables players to use interactable objects.<br></br>
/// Additionally, for the sake of compactness and neatness of the player object, it also handles grabbing and carrying objects.
/// </summary>
public class PlayerInteractor : BallaNetScript
{
    public NetworkVariable<float> MaxCarryWeight = new(0.9f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> Grabbing = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> Interacting = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField, ReadOnly] Interactable lastInteractTarget;
    [SerializeField, ReadOnly] Rigidbody lastGrabTarget;
    [SerializeField, ReadOnly] Rigidbody grabTarget;
    [SerializeField] protected LayerMask grabMask, interactMask;
    [SerializeField] protected float interactRange, interactRadius;
    [SerializeField] protected float timeBetweenInteract;
    float interactDelay;
    PlayerController pc;

    public Transform interactOrigin;

    [Rpc(SendTo.Owner)]
    public void SetInteractFromExternalSource_RPC(bool state, RpcParams data = default)
    {
        if(IsOwner)
            Interacting.Value = state;
    }
    [Rpc(SendTo.Owner)]
    public void SetGrabFromExternalSource_RPC(bool state, RpcParams data = default)
    {
        if (IsOwner)
            Grabbing.Value = state;
    }

    public override void OnNetworkSpawn()
    {
        if(pc == null)
        {
            pc = GetComponent<PlayerController>();
        }
    }

    protected override void Timestep()
    {
        //We don't want to do anything on here if we're not the owner of this object.
        if (!IsOwner)
            return;
        //If we're grabbing, we don't want to be able to interact.
        //We'll check this off first.
        CheckGrab();
    }
    void CheckInteract()
    {

    }
    void CheckGrab()
    {
        if (Grabbing.Value)
        {

        }
    }
}
