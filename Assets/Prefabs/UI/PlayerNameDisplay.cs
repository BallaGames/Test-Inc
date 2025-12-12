using Balla.Core;
using Heathen.SteamworksIntegration;
using Steamworks;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : BallaNetScript
{
    public TMP_Text tmp;
    string pname;
    float t;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CheckName();
    }
    protected override void Timestep()
    {
        if(t <= 30)
        {
            t += Time.fixedDeltaTime;
        }
        else
        {
            CheckName();
        }
        
    }
    void CheckName()
    {
        if (tmp)
        {
            pname = SteamFriends.GetFriendPersonaName(new CSteamID(PlayerController.clientSteamIDs[OwnerClientId]));
            tmp.text = pname;
        }
    }
    protected override void AfterFrame()
    {
        transform.LookAt(Camera.main.transform, Camera.main.transform.up);
    }
}
