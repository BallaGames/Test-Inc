using Balla.Core;
using Unity.Netcode;
using UnityEngine;

public class DefaultSpawns : BallaNetScript
{
    public Transform[] transforms;
    public NetworkObject objectToSpawn;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        for (int i = 0; i < transforms.Length; i++)
        {
            _ = NetworkManager.SpawnManager.InstantiateAndSpawn(objectToSpawn, position: transforms[i].position, rotation: transforms[i].rotation);
        }
    }
}
