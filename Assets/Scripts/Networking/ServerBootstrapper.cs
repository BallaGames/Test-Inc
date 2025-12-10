using Unity.Netcode;
using UnityEngine;

public class ServerBootstrapper : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        NetworkManager.Singleton.OnServerStopped += Singleton_OnServerStopped;
    }

    public NetworkObject[] serverRequiredObjects;
    public bool serverActive;


    private void Singleton_OnServerStopped(bool obj)
    {
        if (NetworkManager.Singleton.IsServer)
        {
        
        }
        serverActive = false;
    }

    private void Singleton_OnServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < serverRequiredObjects.Length; i++)
            {
                _ = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(serverRequiredObjects[i], position: Vector3.zero, rotation: Quaternion.identity);
            }
        }
        serverActive = true;
    }
}
