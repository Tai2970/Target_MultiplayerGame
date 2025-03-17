using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections; // Required for waiting before restart

public class GameTimer : NetworkBehaviour
{
    public TMP_Text timerText; // UI Timer
    private NetworkVariable<float> timeLeft = new NetworkVariable<float>(60f, writePerm: NetworkVariableWritePermission.Server);
    private bool gameEnded = false; // Prevents multiple endings
    private float lastTickTime = 0f; // Track last tick sound

    private bool countdownStarted = false; // Prevents countdown from running multiple times

    private void Start()
    {
        if (IsServer && !countdownStarted)
        {
            countdownStarted = true;
            StartCoroutine(StartCountdown()); // Start 3-second countdown before game starts
        }
    }

    private IEnumerator StartCountdown()
    {
        for (int i = 3; i > 0; i--)
        {
            UpdateCountdownClientRpc(i);
            AudioManager.Instance.PlaySound(AudioManager.Instance.timerTickSound); // Play tick sound for countdown
            yield return new WaitForSeconds(1f);
        }

        StartGameClientRpc(); // Start the game after countdown
    }

    [ClientRpc]
    private void UpdateCountdownClientRpc(int count)
    {
        timerText.text = $"Game Starts In: {count}";
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        timerText.text = "GO!"; // Show "GO!" before starting timer
        AudioManager.Instance.PlaySound(AudioManager.Instance.gameStartSound);
        StartCoroutine(ClearGoText()); // Clear "GO!" after 1 second
    }

    private IEnumerator ClearGoText()
    {
        yield return new WaitForSeconds(1f);
        if (IsServer) timeLeft.Value = 60f; // Set game timer after countdown
        UpdateTimerClientRpc(Mathf.CeilToInt(timeLeft.Value));
    }

    private void Update()
    {
        if (IsServer && timeLeft.Value > 0)
        {
            timeLeft.Value -= Time.deltaTime;

            // Play timer ticking sound every second
            if (Mathf.Floor(timeLeft.Value) != Mathf.Floor(lastTickTime))
            {
                lastTickTime = timeLeft.Value;
                PlayTickSoundClientRpc();
            }
        }

        // Sync timer display across all clients
        UpdateTimerClientRpc(Mathf.CeilToInt(timeLeft.Value));

        // When time reaches 0, declare winner & stop movement
        if (timeLeft.Value <= 0 && !gameEnded)
        {
            gameEnded = true;
            EndMatchClientRpc();
        }
    }

    [ClientRpc]
    private void PlayTickSoundClientRpc()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.timerTickSound); // Timer tick sound
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(int timeRemaining)
    {
        timerText.text = "Time Left: " + timeRemaining;
    }

    [ClientRpc]
    private void EndMatchClientRpc()
    {
        ScoreManager scores = FindObjectOfType<ScoreManager>();
        string resultMessage;

        if (scores.GetPlayer1Score() > scores.GetPlayer2Score())
            resultMessage = "Player 1 Wins!";
        else if (scores.GetPlayer2Score() > scores.GetPlayer1Score())
            resultMessage = "Player 2 Wins!";
        else
            resultMessage = "It's a Draw!";

        timerText.text = resultMessage; // Show winner message

        // Play game end sound
        AudioManager.Instance.PlaySound(AudioManager.Instance.gameEndSound);

        DisablePlayerControls(); // Stop player movement
        StartCoroutine(ReturnToMainMenu()); // Return both Host & Client to Main Menu
    }

    // Stops all players from moving after time runs out
    private void DisablePlayerControls()
    {
        TankMovement[] players = FindObjectsOfType<TankMovement>();
        foreach (TankMovement player in players)
        {
            player.enabled = false; // Disables Tank Movement script
        }
    }

    // Ensure BOTH Host & Client return to the Main Menu and clean up networked objects
    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(5f);

        if (IsServer)
        {
            // Despawn all existing tanks before returning to Main Menu
            TankMovement[] tanks = FindObjectsOfType<TankMovement>();
            foreach (TankMovement tank in tanks)
            {
                if (tank.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
                {
                    networkObject.Despawn();
                    Destroy(tank.gameObject);
                }
            }

            // Shutdown network and reload main menu
            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            // Client manually disconnects and reloads the main menu
            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
