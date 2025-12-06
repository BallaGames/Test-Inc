using Balla.Core;
using Balla.Gameplay.Player;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public class NetPlayerData : BallaNetScript
{
    public static Dictionary<ulong, PlayerInteractor> Interactors = new Dictionary<ulong, PlayerInteractor>();
    public static Dictionary<ulong, PlayerController> PlayerControllers = new Dictionary<ulong, PlayerController>();

    public PlayerInteractor interactor;
    public PlayerController controller;

    public override void OnNetworkSpawn()
    {
        if (interactor == null)
            interactor = GetComponent<PlayerInteractor>();
        if(controller == null)
            controller = GetComponent<PlayerController>();

        Interactors ??= new() { {OwnerClientId, GetComponent<PlayerInteractor>() } };
        PlayerControllers ??= new();
        
    }
}
