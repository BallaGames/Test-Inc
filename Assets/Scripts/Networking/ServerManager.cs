using Balla.Core;
using Netcode.Transports;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : BallaNetScript
{
    public static Dictionary<ulong, SteamNetworkingSocketsTransport.SteamConnectionData> ConnectionMapping => TransportSelector.Instance.steamTransport.ConnectionMapping;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong obj)
    {
        bool val = ConnectionMapping.ContainsKey(obj);
        Debug.Log($"Client Connected to server with ID: {obj} - Matches to Steam Connection? {val.ToString() + (val ? $":{ConnectionMapping[obj]}" : "NONE")}");
    }
}
