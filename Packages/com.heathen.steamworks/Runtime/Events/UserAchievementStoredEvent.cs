﻿#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A custom serializable <see cref="UnityEvent{T0}"/> which handles <see cref="UserAchievementStored_t"/> data.
    /// </summary>
    [Serializable]
    public class UserAchievementStoredEvent : UnityEvent<UserAchievementStored_t>
    { }
}
#endif
