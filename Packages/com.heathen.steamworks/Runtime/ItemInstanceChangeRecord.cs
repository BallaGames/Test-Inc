﻿#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct ItemInstanceChangeRecord
    {
        public SteamItemInstanceID_t instance;
        public bool added;
        public bool removed;
        public bool changed;
        public int quantityBefore;
        public int quantityAfter;
        public int QuantityChange => quantityAfter - quantityBefore;
    }
}
#endif