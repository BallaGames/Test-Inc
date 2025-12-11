using Balla.Core;
using Netcode.Transports;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : BallaNetScript
{
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong obj)
    {


    }
}
