﻿#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Structure of the player entry data returned by the <see cref="GameServerBrowserManager.PlayerDetails(GameServerBrowserEntry, Action{GameServerBrowserEntry, bool})"/> method
    /// </summary>
    [Serializable]
    public class ServerPlayerEntry
    {
        public string name;
        public int score;
        public TimeSpan timePlayed;
    }
}
#endif