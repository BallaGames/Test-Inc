﻿#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks")]
    [DisallowMultipleComponent]
    public class FriendManager : MonoBehaviour
    {
        public bool ListenForFriendsMessages
        {
            get => API.Friends.Client.ListenForFriendsMessages;
            set => API.Friends.Client.ListenForFriendsMessages = value;
        }

        public GameConnectedFriendChatMsgEvent evtGameConnectedChatMsg;
        public FriendRichPresenceUpdateEvent evtRichPresenceUpdated;
        public PersonaStateChangeEvent evtPersonaStateChanged;

        private void OnEnable()
        {
            API.Friends.Client.EventGameConnectedFriendChatMsg.AddListener(evtGameConnectedChatMsg.Invoke);
            API.Friends.Client.EventFriendRichPresenceUpdate.AddListener(evtRichPresenceUpdated.Invoke);
            API.Friends.Client.EventPersonaStateChange.AddListener(evtPersonaStateChanged.Invoke);
        }

        private void OnDisable()
        {
            API.Friends.Client.EventGameConnectedFriendChatMsg.RemoveListener(evtGameConnectedChatMsg.Invoke);
            API.Friends.Client.EventFriendRichPresenceUpdate.RemoveListener(evtRichPresenceUpdated.Invoke);
            API.Friends.Client.EventPersonaStateChange.RemoveListener(evtPersonaStateChanged.Invoke);
        }

        public UserData[] GetFriends(EFriendFlags flags) => API.Friends.Client.GetFriends(flags);
        public UserData[] GetCoplayFriends() => API.Friends.Client.GetCoplayFriends();
        public string GetFriendMessage(UserData userId, int index, out EChatEntryType type) => API.Friends.Client.GetFriendMessage(userId, index, out type);
        public bool SendMessage(UserData friend, string message) => API.Friends.Client.ReplyToFriendMessage(friend, message);
    }
}
#endif