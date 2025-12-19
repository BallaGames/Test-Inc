using Balla.Core;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using System;
using System.Linq;

public class PlayerController : BallaNetScript
{
    public static NetworkDictionary<ulong, ulong> clientSteamIDs = new();
    [SerializeField] protected NetworkObject playerPrefab;
    AuthenticationTicket ticket;
    
    public override void OnNetworkSpawn()
    {
        //We need to verify this user with the server.
        if (IsOwner)
        {
            Authentication.GetAuthSessionTicket(LocalSteamData.UserData, GetAuthTicket);
        }
    }
    void GetAuthTicket(AuthenticationTicket ticket, bool ioError)
    {
        if(!ioError && ticket.Result == EResult.k_EResultOK)
        {
            //Send the ticket response data to the host/server
            this.ticket = ticket;
            SendAuthTicket_RPC(ticket.Data, LocalSteamData.UserData);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            Authentication.EndAllSessions();
        }
        else
        {
            Authentication.EndAuthSession(clientSteamIDs[OwnerClientId]);
            Authentication.CancelAuthTicket(ticket);
            ticket = null;
        }
    }
    [Rpc(SendTo.Server)]
    void SendAuthTicket_RPC(byte[] ticketData, ulong steamID)
    {
        CSteamID sender = new(steamID);
        var requestResult = Authentication.BeginAuthSession(ticketData, sender, AuthTicketProcessed);
    }
    private void AuthTicketProcessed(AuthenticationSession session)
    {
        if(session.Response == EAuthSessionResponse.k_EAuthSessionResponseOK)
        {
            Debug.Log("Spawning player following successful authentication");
            clientSteamIDs.Add(OwnerClientId, session.User);
            NetworkManager.SpawnManager.InstantiateAndSpawn(playerPrefab, OwnerClientId);
        }
        else
        {
            Debug.LogWarning($"TICKET INVALID! REASON : {Enum.GetName(typeof(EAuthSessionResponse), session.Response)}");
            SteamLobbyHelper.currentLobby.KickMember(session.User);
            DenyAuthentication_RPC(session.Response);
        }
    }
    [Rpc(SendTo.Owner)]
    public void DenyAuthentication_RPC(EAuthSessionResponse response)
    {
        Debug.LogWarning($"Auth denied by server. reason: {response}");
        SteamLobbyHelper.Instance.LeftLobby(null);
    }
}
