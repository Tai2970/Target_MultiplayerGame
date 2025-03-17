using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public GameObject tankPrefab; // Assign the Tank Prefab in Unity Inspector
    private Vector2 spawnPosition1 = new Vector2(-3, 0);
    private Vector2 spawnPosition2 = new Vector2(3, 0);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandlePlayerConnection;
        }
    }

    private void HandlePlayerConnection(ulong clientId)
    {
        // Ensure tanks are not duplicated when replaying
        if (!HasTank(clientId))
        {
            SpawnPlayer(clientId);
        }
    }

    private bool HasTank(ulong clientId)
    {
        // Check if a tank already exists for this client
        TankMovement[] tanks = FindObjectsOfType<TankMovement>();
        foreach (TankMovement tank in tanks)
        {
            if (tank.OwnerClientId == clientId)
            {
                return true; // Tank already exists, prevent duplication
            }
        }
        return false;
    }

    private void SpawnPlayer(ulong clientId)
    {
        Vector2 spawnPosition = clientId == 0 ? spawnPosition1 : spawnPosition2;
        GameObject tank = Instantiate(tankPrefab, spawnPosition, Quaternion.identity);

        // Ensure the tank is networked and assigned to the correct client
        NetworkObject tankObject = tank.GetComponent<NetworkObject>();
        tankObject.SpawnAsPlayerObject(clientId);
    }

    public void RestartGame()
    {
        if (IsServer) // Only the Host should reset the game
        {
            StartCoroutine(ResetGameAfterDelay());
        }
    }

    private IEnumerator ResetGameAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Small delay to prevent instant restart

        // Destroy all tanks before restarting
        TankMovement[] tanks = FindObjectsOfType<TankMovement>();
        foreach (TankMovement tank in tanks)
        {
            if (tank.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                networkObject.Despawn(); // Remove old tanks
                Destroy(tank.gameObject);
            }
        }

        // Ensure all networked objects are reset before reloading scene
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
