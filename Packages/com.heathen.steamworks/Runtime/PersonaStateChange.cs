﻿#if !DISABLESTEAMWORKS  && (STEAMWORKSNET || STEAM_LEGACY || STEAM_161 || STEAM_162)
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct PersonaStateChange
    {
        public PersonaStateChange_t data;
        public CSteamID SubjectId => new CSteamID(data.m_ulSteamID);
        public EPersonaChange Flags => data.m_nChangeFlags;

        public static implicit operator PersonaStateChange(PersonaStateChange_t native) => new PersonaStateChange { data = native };
        public static implicit operator PersonaStateChange_t(PersonaStateChange heathen) => heathen.data;
    }
}
#endif