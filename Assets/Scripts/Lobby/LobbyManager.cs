using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private int minPlayersToStart = 2;

    private NetworkVariable<bool> allPlayersReady =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private TMP_Text statusText;
    private TMP_Text hintText;

    public override void OnNetworkSpawn()
    {
        allPlayersReady.OnValueChanged += OnAllPlayersReadyChanged;

        // UI potrzebujemy tylko lokalnie dla hosta
        if (IsHost)
        {
            LobbyStatusPanel lobbyUI =
                FindAnyObjectByType<LobbyStatusPanel>();

            if (lobbyUI != null)
            {
                statusText = lobbyUI.statusText;
                hintText = lobbyUI.hintText;
            }

            UpdateHostUI(allPlayersReady.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        allPlayersReady.OnValueChanged -= OnAllPlayersReadyChanged;
    }

    private void Update()
    {
        if (!IsHost)
            return;

        if (!allPlayersReady.Value)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    public void CheckAllPlayersReady()
    {
        if (!IsServer)
            return;

        int connectedPlayers =
            NetworkManager.Singleton.ConnectedClientsList.Count;

        Debug.Log(
            "CONNECTED PLAYERS = " +
            connectedPlayers
        );

        // Musi być minimum 2 graczy
        if (connectedPlayers < minPlayersToStart)
        {
            allPlayersReady.Value = false;

            Debug.Log(
                "Not enough players. Waiting for another player."
            );

            return;
        }

        bool everyoneReady = true;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
            {
                everyoneReady = false;
                break;
            }

            LobbyReadySystem readySystem =
                client.PlayerObject.GetComponent<LobbyReadySystem>();

            if (readySystem == null)
            {
                everyoneReady = false;
                break;
            }

            Debug.Log(
                "Client " +
                client.ClientId +
                " READY = " +
                readySystem.IsReady
            );

            if (!readySystem.IsReady)
            {
                everyoneReady = false;
                break;
            }
        }

        allPlayersReady.Value = everyoneReady;

        Debug.Log(
            "ALL PLAYERS READY = " +
            everyoneReady
        );
    }

    private void OnAllPlayersReadyChanged(
        bool oldValue,
        bool newValue)
    {
        if (!IsHost)
            return;

        UpdateHostUI(newValue);
    }

    private void UpdateHostUI(bool allReady)
    {
        if (statusText == null || hintText == null)
            return;

        NetworkObject localPlayer =
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

        if (localPlayer == null)
            return;

        LobbyReadySystem readySystem =
            localPlayer.GetComponent<LobbyReadySystem>();

        if (readySystem == null)
            return;

        // Wszyscy gracze są READY
        if (allReady)
        {
            statusText.text = "ALL PLAYERS READY";
            hintText.text = "Press [ENTER] to Start";
            return;
        }

        // Host nie jest READY
        if (!readySystem.IsReady)
        {
            statusText.text = "NOT READY";
            hintText.text = "Press [P] to Ready";
            return;
        }

        // Host jest READY, ale ktoś jeszcze nie
        statusText.text = "READY";
        hintText.text = "Waiting for all players";
    }

    private void StartGame()
    {
        if (!IsServer)
            return;

        Debug.Log("STARTING GAME!");

        NetworkManager.Singleton.SceneManager.LoadScene(
            "GameScene",
            LoadSceneMode.Single
        );
    }
}