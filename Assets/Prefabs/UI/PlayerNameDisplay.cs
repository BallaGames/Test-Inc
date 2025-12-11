using Balla.Core;
using Heathen.SteamworksIntegration;
using Steamworks;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : BallaNetScript
{
    public TMP_Text tmp;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (tmp)
        {
            tmp.text = SteamFriends.GetFriendPersonaName(new CSteamID(PlayerController.clientSteamIDs[OwnerClientId]));
        }
    }

    protected override void AfterFrame()
    {
        transform.LookAt(Camera.main.transform, Camera.main.transform.up);
    }
}
