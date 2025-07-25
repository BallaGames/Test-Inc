﻿#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using UnityEngine;
using Steamworks;
using UserAPI = Heathen.SteamworksIntegration.API.User.Client;
using FriendsAPI = Heathen.SteamworksIntegration.API.Friends.Client;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Applies the name of the indicated user to the attached label
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class UGUISetUserName : MonoBehaviour
    {
        private UnityEngine.UI.Text label;
        [SerializeField]
        [Tooltip("Should the component load the local user's name on Start.\nIf false you must call SetName and provide the ID of the user to load")]
        private bool useLocalUser;

        public bool ShowNickname
        {
            get
            {
                return showNickname;
            }
            set
            {
                showNickname = value;
                SetName(currentUser);
            }
        }

        [SerializeField]
        [Tooltip("Should we show the profile name (set by the user this represents) or the nick name (set by the local user for this user)")]
        private bool showNickname;

        public UserData UserData
        {
            get
            {
                return currentUser;
            }
            set
            {
                SetName(value);
            }
        }

        private UserData currentUser;

        private void OnEnable()
        {
            FriendsAPI.EventPersonaStateChange.AddListener(HandlePersonaStateChange);
        }

        private void OnDisable()
        {
            FriendsAPI.EventPersonaStateChange.RemoveListener(HandlePersonaStateChange);
        }

        private void Start()
        {
            label = GetComponent<UnityEngine.UI.Text>();

            if (useLocalUser)
            {
                if (API.App.Initialized)
                    SetName(UserData.Me);
                else
                    API.App.evtSteamInitialized.AddListener(HandleSteamInitalized);

            }
        }

        private void HandleSteamInitalized()
        {
            if (useLocalUser)
                SetName(UserData.Me);

            API.App.evtSteamInitialized.RemoveListener(HandleSteamInitalized);
        }

        private void HandlePersonaStateChange(PersonaStateChange arg)
        {
            UserData user = arg.SubjectId;
            if ((FriendsAPI.PersonaChangeHasFlag(arg.Flags, EPersonaChange.k_EPersonaChangeName) || FriendsAPI.PersonaChangeHasFlag(arg.Flags, EPersonaChange.k_EPersonaChangeNickname))
                && user == currentUser)
            {
                if (showNickname)
                    label.text = user.Nickname;
                else
                    label.text = user.Name;
            }
        }

        public void SetName(UserData user)
        {
            if (label == null)
                label = GetComponent<UnityEngine.UI.Text>();

            if (label == null)
                return;

            currentUser = user;

            if (showNickname)
                label.text = user.Nickname;
            else
                label.text = user.Name;
        }

        public void SetName(CSteamID user)
        {
            if (label == null)
                label = GetComponent<UnityEngine.UI.Text>();

            if (label == null)
                return;

            currentUser = user;

            if (showNickname)
                label.text = UserData.Get(user).Nickname;
            else
                label.text = UserData.Get(user).Name;
        }

        public void SetName(ulong user)
        {
            if (label == null)
                label = GetComponent<UnityEngine.UI.Text>();

            if (label == null)
                return;

            currentUser = user;

            if (showNickname)
                label.text = UserData.Get(user).Nickname;
            else
                label.text = UserData.Get(user).Name;
        }
    }
}
#endif