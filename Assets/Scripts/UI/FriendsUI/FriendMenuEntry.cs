using Heathen.SteamworksIntegration;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendMenuEntry : MonoBehaviour
{
    public Image avatar;
    public UserData currentUser;
    public TMP_Text text;
    public Sprite spt;
    Texture2D avt;
    public void Initialise(UserData user)
    {
        text.text = user.Name;
        if (user.InGame)
        {
            text.color = Color.green;
        }
        else
        {
            text.color = user.State switch
            {
                EPersonaState.k_EPersonaStateOffline => Color.grey,
                EPersonaState.k_EPersonaStateOnline => Color.cyan,
                EPersonaState.k_EPersonaStateBusy => Color.darkCyan,
                EPersonaState.k_EPersonaStateAway => Color.darkCyan,
                EPersonaState.k_EPersonaStateSnooze => Color.darkCyan,
                EPersonaState.k_EPersonaStateLookingToTrade => Color.cyan,
                EPersonaState.k_EPersonaStateLookingToPlay => Color.cyan,
                EPersonaState.k_EPersonaStateInvisible => Color.grey,
                EPersonaState.k_EPersonaStateMax => Color.cyan,
                _ => Color.grey,
            };
        }
        avt = SteamHelper.GetSteamImageAsTexture2D(SteamFriends.GetMediumFriendAvatar(user.id));
        spt = Sprite.Create(avt, new(0, 0, avt.width, avt.height), new(0.5f, 0.5f));
        avatar.sprite = spt;
    }
    private void OnDestroy()
    {
        spt = null;
        Destroy(avt);
    }
}
