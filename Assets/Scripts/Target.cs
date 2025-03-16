using UnityEngine;
using Unity.Netcode;

public class Target : NetworkBehaviour
{
    public float moveSpeed = 1f;
    public float lifetime = 10f;
    private Vector2 moveDirection;
    private NetworkVariable<Vector3> targetPosition = new NetworkVariable<Vector3>();

    public GameObject explosionEffectPrefab; 

    private void Start()
    {
        if (IsServer) 
        {
            SetRandomMovement();
            Invoke(nameof(RespawnTarget), lifetime);
        }
    }

    private void Update()
    {
        if (IsServer) 
        {
            Vector3 newPosition = transform.position + (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
            targetPosition.Value = newPosition; 
            transform.position = newPosition;
        }
        else 
        {
            transform.position = targetPosition.Value;
        }
    }

    private void SetRandomMovement()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);
        moveDirection = new Vector2(randomX, randomY).normalized;
    }

    private void RespawnTarget()
    {
        if (!IsServer) return;

        // Move target to a new random position (Force Z = 0)
        float newX = Random.Range(-6f, 6f);
        float newY = Random.Range(-4f, 4f);
        transform.position = new Vector3(newX, newY, 0); 

        // Sync position with clients
        targetPosition.Value = transform.position;

        // Set a new movement direction
        SetRandomMovement();

        // Restart the timer for the next respawn
        Invoke(nameof(RespawnTarget), lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return; 

        if (collision.CompareTag("Bullet")) 
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null)
            {
                ulong shooterId = bullet.GetShooter(); 
                ScoreManager.Instance.AddScoreServerRpc(shooterId); 

                // Play target hit sound
                AudioManager.Instance.PlaySound(AudioManager.Instance.targetHitSound);

                // Show explosion effect before destroying target
                ShowDestructionEffectClientRpc(transform.position);

                collision.GetComponent<NetworkObject>().Despawn(true); 
                Destroy(collision.gameObject);

                RespawnTarget(); 
            }
        }
    }

    [ClientRpc]
    private void ShowDestructionEffectClientRpc(Vector3 position)
    {
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1.5f); 
        }
    }
}
