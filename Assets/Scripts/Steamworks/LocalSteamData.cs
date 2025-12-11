using Balla.Core;
using Heathen.SteamworksIntegration;
using Steamworks;
using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public class LocalSteamData : BallaScript
{
    public static LocalSteamData Instance;
    public static UserData UserData { get; private set; }
    public static FixedString128Bytes SteamName { get; private set; }
    public static bool SteamInitialised => SteamAPI.IsSteamRunning();
    public static Texture2D SteamAvatar;
    public SteamLobbyHelper lobbyHelper;
    public static Action OnInitialise;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = GetComponent<LocalSteamData>();
        }
        else
        {
            return;
        }
    }
    private async void Start()
    {
        await Task.Delay(20);
        UserData = UserData.Me;
        SteamName = SteamFriends.GetPersonaName();
        SteamAvatar = SteamHelper.GetSteamImageAsTexture2D(SteamFriends.GetMediumFriendAvatar(UserData));

        OnInitialise?.Invoke();
    }


    public static string GetNameFromID(ulong ID)
    {
        return SteamFriends.GetFriendPersonaName(new CSteamID(ID));
    }

    private void OnDestroy()
    {
        SteamAPI.Shutdown();
    }
}
