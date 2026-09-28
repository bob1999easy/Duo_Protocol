using UnityEngine;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] private int minPlayersToStart = 2;

    public void CheckAllPlayersReady()
    {
        if (!IsServer)
            return;

        int connectedPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;

        Debug.Log("CONNECTED PLAYERS = " + connectedPlayers);

        // Nie mozna rozpoczac gry, jesli nie ma wymaganej liczby graczy
        if (connectedPlayers < minPlayersToStart)
        {
            Debug.Log("Not enough players. Waiting for another player.");
            return;
        }

        bool everyoneReady = true;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log(
                "Checking Client ID = " +
                client.ClientId
            );

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

        Debug.Log(
            "ALL PLAYERS READY = " +
            everyoneReady
        );
    }
}