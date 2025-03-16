using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // Added for returning to Main Menu

public class NetworkUIManager : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    void Start()
    {
        hostButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);
    }

    void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host started, waiting for clients...");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Hide buttons after starting as Host
            hostButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to start as Host.");
        }
    }

    void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Attempting to connect as Client...");

            // Start a coroutine to check if the connection fails
            StartCoroutine(CheckClientConnection());
        }
        else
        {
            Debug.LogError("Failed to start as Client. No Host found!");
            ReturnToMainMenu(); // Instead of quitting, return to Main Menu
        }
    }

    private IEnumerator CheckClientConnection()
    {
        yield return new WaitForSeconds(2f); // Wait a bit to see if connection succeeds

        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            Debug.LogError("No Host found! Returning to menu...");
            ReturnToMainMenu(); // Instead of quitting, return to Main Menu
        }
        else
        {
            Debug.Log("Connected as Client");
            NetworkManager.Singleton.OnClientDisconnectCallback += OnDisconnectedFromServer;

            // Hide buttons after starting as Client
            hostButton.gameObject.SetActive(false);
            clientButton.gameObject.SetActive(false);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("A client has joined! Client ID: " + clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log("A client has disconnected. Client ID: " + clientId);
    }

    private void OnDisconnectedFromServer(ulong clientId)
    {
        Debug.Log("Disconnected from Host. Returning to menu...");
        ReturnToMainMenu(); // Instead of quitting, return to Main Menu
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("Returning to Main Menu...");
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown(); // Disconnect from the network
        }
        SceneManager.LoadScene("MainMenu"); // Load Main Menu Scene instead of quitting
    }
}
