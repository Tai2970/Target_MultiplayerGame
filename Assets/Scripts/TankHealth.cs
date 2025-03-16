using UnityEngine;
using Unity.Netcode;

public class TankHealth : NetworkBehaviour
{
    public int maxHealth = 3;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsServer) // Only the server initializes health
        {
            currentHealth.Value = maxHealth;
        }
    }

    [ServerRpc(RequireOwnership = false)] // Allow both Host & Client to take damage
    public void TakeDamageServerRpc()
    {
        if (currentHealth.Value > 0)
        {
            currentHealth.Value -= 1;
            Debug.Log($"Tank {OwnerClientId} took damage! HP: {currentHealth.Value}");

            // Play explosion sound when tank takes damage
            AudioManager.Instance.PlaySound(AudioManager.Instance.explosionSound);

            if (currentHealth.Value <= 0)
            {
                DestroyTank();
            }
        }
    }

    private void DestroyTank()
    {
        Debug.Log($"Tank {OwnerClientId} destroyed!");

        // Play tank destroyed sound
        AudioManager.Instance.PlaySound(AudioManager.Instance.tankDestroyedSound);

        GetComponent<NetworkObject>().Despawn(true);
        gameObject.SetActive(false);
    }
}
