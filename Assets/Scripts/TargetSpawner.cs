using UnityEngine;
using Unity.Netcode;

public class TargetSpawner : NetworkBehaviour
{
    public GameObject targetPrefab;
    public int numberOfTargets = 3;

    public override void OnNetworkSpawn()
    {
        if (IsServer) 
        {
            for (int i = 0; i < numberOfTargets; i++)
            {
                SpawnTarget();
            }
        }
    }

    private void SpawnTarget()
    {
        if (!IsServer) return; 

        float spawnX = Random.Range(-6f, 6f);
        float spawnY = Random.Range(-4f, 4f);
        GameObject newTarget = Instantiate(targetPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);

        NetworkObject targetNetObj = newTarget.GetComponent<NetworkObject>();

        if (targetNetObj != null) 
        {
            targetNetObj.Spawn();
        }
        else
        {
            Debug.LogError("Target prefab is missing a NetworkObject component!");
        }
    }
}
