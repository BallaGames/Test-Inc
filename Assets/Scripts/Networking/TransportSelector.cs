using Netcode.Transports;
using Unity.Netcode;
using Unity.Netcode.Transports.SinglePlayer;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class TransportSelector : MonoBehaviour
{

    public NetworkManager networkManager;

    public SinglePlayerTransport spTransport;
    public UnityTransport unityTransport;
    public SteamNetworkingSocketsTransport steamTransport;

    public static TransportSelector Instance;
    Transport currentTransport;
    public bool useGUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
        else
        {
            return;
        }
    }

    public void SetCurrentTransport(Transport next)
    {
        if (next == Transport.none)
        {
            Debug.LogWarning("Make sure you select a transport from the enum!");
            return;
        }
        networkManager.NetworkConfig.NetworkTransport = next switch
        {
            Transport.none => null, 
            Transport.local => unityTransport,
            Transport.singleplayer => spTransport, 
            Transport.steam => steamTransport,
            _ => null
        };
    }

    void Init()
    {
        if (spTransport == null)
            spTransport = GetComponentInChildren<SinglePlayerTransport>();
        if (unityTransport == null)
            unityTransport = GetComponentInChildren<UnityTransport>();
        if (steamTransport == null)
            steamTransport = GetComponentInChildren<SteamNetworkingSocketsTransport>();

        networkManager = GetComponent<NetworkManager>();
    }

    public enum Transport
    {
        none = 0,
        local = 1,
        singleplayer = 2,
        steam = 3,
    }

    private void OnGUI()
    {
        float scaleX = Screen.width / 1280f;
        float scaleY = Screen.height / 720f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX, scaleY, 1));
        GUILayout.BeginVertical();
        GUILayout.Space(100 * scaleY);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<"))
        {
            int index = (int)currentTransport;
            index = (int)Mathf.Repeat(index - 1, 4);
            currentTransport = (Transport)index;
            SetCurrentTransport(currentTransport);
        }
        GUILayout.Label(System.Enum.GetName(typeof(Transport), currentTransport));
        if (GUILayout.Button(">"))
        {
            int index = (int)currentTransport;
            index = (int)Mathf.Repeat(index + 1, 4);
            currentTransport = (Transport)index;
            SetCurrentTransport(currentTransport);
        }
        GUILayout.EndHorizontal();

        if (!(networkManager.IsServer || networkManager.IsClient))
        {
            switch (currentTransport)
            {
                case Transport.none:
                    GUILayout.Label("No transport selected.");
                    break;
                case Transport.local:
                    if(GUILayout.Button("Start Host"))
                    {
                        networkManager.StartHost();
                    }
                    if(GUILayout.Button("Start Client"))
                    {
                        networkManager.StartClient();
                    }
                    break;
                case Transport.singleplayer:
                    if(GUILayout.Button("Start Game"))
                    {
                        networkManager.StartHost();
                    }
                    break;
                case Transport.steam:
                    if(GUILayout.Button("Start Host via Steam"))
                    {
                        SteamLobbyHelper.Instance.CreateLobby(Steamworks.ELobbyType.k_ELobbyTypeFriendsOnly);
                        networkManager.StartHost();
                    }
                    GUILayout.Label("Must join via STEAM to play!");
                    break;
                default:
                    GUILayout.Label("No transport selected.");
                    break;
            }
        }
        else
        {
            GUILayout.Label($"Clients Connected: {networkManager.ConnectedClientsIds.Count}");
            if (networkManager.ShutdownInProgress)
            {
                GUILayout.Label("Shutting down Network Manager");
            }
            if (GUILayout.Button("Shutdown!"))
            {
                networkManager.Shutdown();
                if(currentTransport == Transport.steam)
                {
                    SteamLobbyHelper.Instance.LeaveLobby(SteamLobbyHelper.currentLobby);
                }
            }
        }
        GUILayout.EndVertical();
    }
}
