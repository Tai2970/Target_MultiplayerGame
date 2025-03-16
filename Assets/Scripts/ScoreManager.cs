using UnityEngine;
using Unity.Netcode;
using TMPro; // Import TextMeshPro

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;
    public TMP_Text scoreText; // Now using TextMeshPro

    private NetworkVariable<int> player1Score = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<int> player2Score = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        // Hide score at the start
        if (scoreText != null)
        {
            scoreText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Show score once the game starts (Host or Client joins)
        if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost)
        {
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(true);
            }
        }

        // Update UI text to display both scores
        if (scoreText != null)
        {
            scoreText.text = $"Player 1: {player1Score.Value}   Player 2: {player2Score.Value}";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(ulong playerId)
    {
        if (playerId == 0) // Host (Player 1)
        {
            player1Score.Value += 1;
        }
        else // Client (Player 2)
        {
            player2Score.Value += 1;
        }

        // Play gain point sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.gainPointSound);
        }
    }

    // Reset scores when returning to Main Menu
    [ServerRpc(RequireOwnership = false)]
    public void ResetScoresServerRpc()
    {
        player1Score.Value = 0;
        player2Score.Value = 0;
    }

    // Call this when returning to Main Menu
    public void ResetScores()
    {
        if (IsServer)
        {
            ResetScoresServerRpc();
        }
    }

    // Getter Functions for Timer
    public int GetPlayer1Score()
    {
        return player1Score.Value;
    }

    public int GetPlayer2Score()
    {
        return player2Score.Value;
    }
}
