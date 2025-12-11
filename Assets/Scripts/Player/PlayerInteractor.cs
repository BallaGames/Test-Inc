using Balla;
using Balla.Core;
using Balla.Gameplay.Player;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// This script enables players to use interactable objects.<br></br>
/// Additionally, for the sake of compactness and neatness of the player object, it also handles grabbing and carrying objects.
/// </summary>
public class PlayerInteractor : BallaNetScript
{
    /// <summary>
    /// The mass at which you can no longer pick something up.
    /// </summary>
    public NetworkVariable<float> maxCarryMass = new(4f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    /// <summary>
    /// The force applied to objects you are carrying
    /// </summary>
    public NetworkVariable<float> CarryForce = new(0.9f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    /// <summary>
    /// Whether this player is currently grabbing something
    /// </summary>
    public NetworkVariable<bool> Grabbing = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    /// <summary>
    /// Whether this player is currently interacting with something
    /// </summary>
    public NetworkVariable<bool> Interacting = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField, ReadOnly] protected Interactable lastInteractTarget, interactTarget, currentInteractable;
    [SerializeField, ReadOnly] protected Rigidbody lastGrabTarget;
    [SerializeField, ReadOnly] protected Rigidbody grabTarget, currentGrabbed;
    [SerializeField] protected LayerMask grabMask, interactMask;
    [SerializeField] protected float carryDrag;
    [SerializeField] protected float grabForce;
    [SerializeField] protected float interactRange, interactRadius;
    [SerializeField] protected float timeBetweenInteract;

    protected float interactDelay;
    PlayerMotor pc;
    public Transform grabPoint;
    public Transform interactOrigin;

    private void Awake()
    {

    }

    [Rpc(SendTo.Owner)]
    public void SetInteractFromExternalSource_RPC(bool state, RpcParams data = default)
    {
        if (IsOwner)
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
        if (pc == null)
        {
            pc = GetComponent<PlayerMotor>();
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
        if (!Grabbing.Value)
            CheckInteract();
    }
    void CheckInteract()
    {
        if (currentInteractable != null)
        {
            //if we're currently interacting with something...
            if (!Input.AltAttack || !CastInteractRay(out RaycastHit hit) || hit.rigidbody == null || hit.collider.attachedRigidbody != currentInteractable.rb)
            {
                currentInteractable.SetInteract_RPC(false);
                currentInteractable = null;
            }
        }
        else
        {
            //We're not interacting with something right now
            if (CastInteractRay(out RaycastHit hit))
            {
                if (hit.rigidbody != null)
                {
                    if (hit.rigidbody.TryGetComponent(out Interactable i))
                    {
                        if (interactTarget == null || lastInteractTarget != interactTarget)
                        {
                            interactTarget = i;
                        }
                        if (Input.AltAttack)
                        {
                            i.SetInteract_RPC(true);
                            currentInteractable = i;
                        }
                    }
                    else
                    {
                        interactTarget = null;
                        //No interactable component present
                    }
                }
                else
                {
                    interactTarget = null;
                    //Hit rigidbody is null, so this can't be an interactable
                }
            }
            else
            {
                interactTarget = null;
            }
        }
        lastInteractTarget = interactTarget;
    }
    bool CastInteractRay(out RaycastHit hit)
    {
        return Physics.SphereCast(interactOrigin.position, interactRadius, interactOrigin.forward, out hit, interactRange, interactMask);
    }
    void CheckGrab()
    {
        if (currentGrabbed)
        {
            currentGrabbed.AddForce(((grabPoint.position - currentGrabbed.position) * CarryForce.Value) - (currentGrabbed.linearVelocity * carryDrag));
            if (!Input.Attack)
            {
                pc.rb.mass -= currentGrabbed.mass;
                currentGrabbed = null;
                Grabbing.Value = false;
            }
        }
        else if (Physics.SphereCast(interactOrigin.position, interactRadius, interactOrigin.forward, out RaycastHit hit, interactRange, grabMask))
        {
            //hit something, compare it to the previous grabbed
            if (hit.rigidbody != null && hit.rigidbody.mass < maxCarryMass.Value)
            {
                grabTarget = hit.rigidbody;
                if (Input.Attack && hit.rigidbody.TryGetComponent(out NetworkObject n))
                {
                    Grabbing.Value = true;
                    currentGrabbed = hit.rigidbody;
                    pc.rb.mass += currentGrabbed.mass;
                    SendParentRequestOnGrab_RPC(n);
                }
            }
        }
        lastGrabTarget = currentGrabbed;
    }
    [Rpc(SendTo.Server)]
    public void SendParentRequestOnGrab_RPC(NetworkObjectReference obj, RpcParams data = default)
    {
        if (obj.TryGet(out NetworkObject nob))
        {
            nob.ChangeOwnership(data.Receive.SenderClientId);
        }
    }
}
