using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using JetBrains.Annotations;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class SteamLobbyHelper : MonoBehaviour
{
    public static SteamLobbyHelper Instance;
    public static LobbyData currentLobby;

    public string[] testKeys = new string[]
    {
        "oooh working it",
        "testing my inc",
        "dont call me inc",
        "secret bana :3"
    };

    public enum RichPresenceKey
    {
        status,
        connect,
        steam_display,
        steam_player_group,
        steam_player_group_size,
    }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
        Matchmaking.Client.EventLobbyInvite.AddListener(ReceivedInvite);
        Overlay.Client.EventGameLobbyJoinRequested.AddListener(HandleLobbyJoinRequest);
        Matchmaking.Client.EventLobbyLeave.AddListener(LeftLobby);
        Matchmaking.Client.EventLobbyAskedToLeave.AddListener(AskedToLeaveLobby);


        Matchmaking.Client.EventLobbyChatUpdate.AddListener(HandleChatUpdate);
        LocalSteamData.OnInitialise += Initialise;
    }
    public void LeftLobby(LobbyData lobby)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            currentLobby = null;
            SteamFriends.ClearRichPresence();
        }
    }
    public void AskedToLeaveLobby(LobbyData lobby)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            currentLobby = null;
            SteamFriends.ClearRichPresence();
        }
    }

    public void SetRandomPresence()
    {
        int random = Random.Range(0, testKeys.Length);
        SetRichPresence(testKeys[random]);
    }

    public void SetRichPresence(string message = "")
    {
        SteamFriends.SetRichPresence("status", message);
    }

    public void Initialise()
    {
        LobbyData targetLobby = Matchmaking.Client.GetCommandLineConnectLobby();
        if (targetLobby.IsValid)
        {
            // we joined someone with the game closed.
            // lets handle that
            // i swear my laptop is turning into a snake sometimes...
            // join the game when its in an appropriate state to join.
            JoinLobby(targetLobby);
        }
    }
    private void OnDestroy()
    {
        Matchmaking.Client.EventLobbyInvite.RemoveListener(ReceivedInvite);
        Overlay.Client.EventGameLobbyJoinRequested.RemoveListener(HandleLobbyJoinRequest);
    }

    public void CreateLobby(ELobbyType lobbyType, int slots = 4)
    {
        LobbyData.Create(lobbyType, slots, HandleLobbyCreate);
    }
    public void HandleLobbyCreate(EResult result, LobbyData data, bool ioError)
    {
        if (ioError)
        {
            Debug.LogWarning("An error occurred whilst creating a lobby! more info available below!");
            Debug.LogWarning($"Error Name - {System.Enum.GetName(typeof(EResult), result)}");
            return;
        }
        data.SetJoinable(true);
        
        SetLobbyMetadata(data);
        SetRandomPresence();
        currentLobby = data;
    }

    public void JoinLobby(LobbyData lobby)
    {
        lobby.Join(HandleJoined);
    }
    void HandleJoined(LobbyEnter Result, bool IOError)
    {
        // Result.Lobby is the lobby joined if any
        // Result.Response to the response message, if any
        // Result.Locked is this lobby locked?
        if (IOError || Result.Locked)
        {
            Debug.LogWarning("Cannot Join Lobby!");
        }
        //This should set the lobby, right?
        TransportSelector.Instance.SetCurrentTransport(TransportSelector.Transport.steam);
        TransportSelector.Instance.steamTransport.ConnectToSteamID = Result.Lobby.Owner.user.id.m_SteamID;
        NetworkManager.Singleton.StartClient();
        SetRandomPresence();
    }

    public void LeaveLobby(LobbyData lobby)
    {
        if(lobby.Owner.user == UserData.Me)
        {
            foreach (var item in lobby.Members)
            {
                //kick all the other users from the game
                lobby.KickMember(item.user);
            }
            //then kick the host
            lobby.KickMember(UserData.Me);
        }
        currentLobby = null;
        SteamFriends.ClearRichPresence();
    }

    public void ReceivedInvite(LobbyInvite response)
    {
        //response.ForGame -- the game the invite is sent for
        //response.FromUser -- the user that invited you
        //response.toLobby -- the lobby you were invited to

        Debug.Log($"INVITE FOR {response.ForGame.Name} FROM {response.FromUser.Name}, OWNED BY {response.ToLobby.Owner.user.Name}");
    }
    public void HandleLobbyJoinRequest(LobbyData lobby, UserData user)
    {
        // lobby is the lobby we're joining
        // user is the user that invited you
        Debug.Log($"ATTEMPTING TO JOIN LOBBY OWNED BY {lobby.Owner.user.Name} - INVITED BY {user.Name}");
        JoinLobby(lobby);
    }

    public void SetLobbyMetadata(LobbyData lobby)
    {
        lobby["Simple field"] = "simple value";
        LobbyMemberData Me = lobby.Me;
        Me["lobby player data"] = "lobby player value";
        LobbyMemberData owner = lobby.Owner;
        owner["lobby owner data"] = "lobby owner value";
    }

    public void GetLobbyMembers(LobbyData lobby)
    {
        foreach (var item in lobby.Members)
        {
            string memberName = item.user.Name;
            string readMetaValues = item["lobby player data"];
            if (item.IsReady)
            {
                //The member is ready!
                Debug.Log($"Player {memberName} is ready!");
            }
            else
            {
                //The member is NOT ready
                Debug.Log($"Player {memberName} is NOT ready!");
            }
        }
    }

    public void HandleChatUpdate(LobbyChatUpdate_t callback)
    {
        if ((EChatMemberStateChange)callback.m_rgfChatMemberStateChange == EChatMemberStateChange.k_EChatMemberStateChangeLeft)
        {
            // The user left
            Debug.Log($"user {LocalSteamData.GetNameFromID(callback.m_ulSteamIDUserChanged)} has left");
        }
        else if ((EChatMemberStateChange)callback.m_rgfChatMemberStateChange == EChatMemberStateChange.k_EChatMemberStateChangeEntered)
        {
            // The user joined
            Debug.Log($"user {LocalSteamData.GetNameFromID(callback.m_ulSteamIDUserChanged)} has joined!");

        }
        else if ((EChatMemberStateChange)callback.m_rgfChatMemberStateChange == EChatMemberStateChange.k_EChatMemberStateChangeDisconnected)
        {
            // The user lost connection
            Debug.Log($"user {LocalSteamData.GetNameFromID(callback.m_ulSteamIDUserChanged)} has disconnected accidentally");
            
        }
    }

}
