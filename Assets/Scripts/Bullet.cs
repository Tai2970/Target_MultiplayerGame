using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour
{
    private ulong shooterId; // Store the shooter’s ID

    public void SetShooter(ulong shooterClientId)
    {
        shooterId = shooterClientId;
    }

    public ulong GetShooter() // Added this method so targets can detect who shot them
    {
        return shooterId;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return; // Only the server handles collisions

        if (collision.CompareTag("Tank")) // Bullet hit a tank
        {
            TankHealth tankHealth = collision.GetComponent<TankHealth>();

            if (tankHealth != null && tankHealth.OwnerClientId != shooterId) 
            {
                tankHealth.TakeDamageServerRpc();

                // Destroy the bullet after impact
                GetComponent<NetworkObject>().Despawn(true);
                Destroy(gameObject);
            }
        }
    }
}
