using UnityEngine;
using Unity.Netcode;

public class TankShooting : NetworkBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;

    void Update()
    {
        if (!IsOwner) return; 

        if (Input.GetKeyDown(KeyCode.Space) && OwnerClientId == 0) // Host (Player 1)
        {
            ShootServerRpc();
        }
        else if (Input.GetKeyDown(KeyCode.Return) && OwnerClientId != 0) // Client (Player 2)
        {
            ShootServerRpc();
        }
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        NetworkObject bulletNetObj = bullet.GetComponent<NetworkObject>();

        if (bulletNetObj == null)
        {
            Debug.LogError("Bullet does not have a NetworkObject component!");
            return;
        }

        bulletNetObj.Spawn(); 

        // Assign the shooter so the bullet knows who fired it
        bullet.GetComponent<Bullet>().SetShooter(OwnerClientId);

        
        if (bulletNetObj.IsSpawned)
        {
            bullet.GetComponent<Rigidbody2D>().linearVelocity = firePoint.up * bulletSpeed;
        }

        // Play shooting sound
        AudioManager.Instance.PlaySound(AudioManager.Instance.shootSound);

        // Tell all clients to sync this bullet
        ShootClientRpc(bulletNetObj.NetworkObjectId);
    }

    [ClientRpc]
    private void ShootClientRpc(ulong bulletId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bulletId, out NetworkObject bulletObject))
        {
            // Ensure bullet exists before applying physics
            if (bulletObject != null)
            {
                bulletObject.GetComponent<Rigidbody2D>().linearVelocity = firePoint.up * bulletSpeed;
            }
        }
    }
}
