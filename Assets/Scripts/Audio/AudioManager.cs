
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Volume Configs")]
    [SerializeField]
    AK.Wwise.RTPC masterVolume;
    AK.Wwise.RTPC sfxVolume;
    AK.Wwise.RTPC musicVolume;

    void SetMasterVolume(float value)
    {
        masterVolume.SetValue(null,value);
    }
    void SetSFXVolume(float value)
    {
        sfxVolume.SetValue(null,value);
    }

    void SetMusicVolume(float value)
    {
        musicVolume.SetValue(null,value);
    }
}