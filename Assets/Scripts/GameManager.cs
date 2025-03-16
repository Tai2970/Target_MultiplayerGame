using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public GameObject tankPrefab; // Assign the Tank Prefab in Unity Inspector
    private Vector2 spawnPosition1 = new Vector2(-3, 0);
    private Vector2 spawnPosition2 = new Vector2(3, 0);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        Vector2 spawnPosition = clientId == 0 ? spawnPosition1 : spawnPosition2;
        GameObject tank = Instantiate(tankPrefab, spawnPosition, Quaternion.identity);

        // Make sure the tank is networked and assigned to the correct client
        NetworkObject tankObject = tank.GetComponent<NetworkObject>();
        tankObject.SpawnAsPlayerObject(clientId);
    }
}
