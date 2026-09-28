using UnityEngine;
using Unity.Netcode;

public class LobbyManager : NetworkBehaviour
{
    public void CheckAllPlayersReady()
    {
        if (!IsServer)
            return;

        bool everyoneReady = true;

        Debug.Log(
            "CONNECTED PLAYERS = " +
            NetworkManager.Singleton.ConnectedClientsList.Count
        );

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Debug.Log(
                "Checking Client ID = " +
                client.ClientId
            );

            if (client.PlayerObject == null)
            {
                Debug.LogError(
                    "Client " +
                    client.ClientId +
                    " HAS NO PLAYER OBJECT!"
                );

                everyoneReady = false;
                continue;
            }

            Debug.Log(
                "Client " +
                client.ClientId +
                " PlayerObject = " +
                client.PlayerObject.name
            );

            LobbyReadySystem readySystem =
                client.PlayerObject.GetComponent<LobbyReadySystem>();

            if (readySystem == null)
            {
                Debug.LogError(
                    "Client " +
                    client.ClientId +
                    " HAS NO LobbyReadySystem!"
                );

                everyoneReady = false;
                continue;
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
            }
        }

        Debug.Log(
            "ALL PLAYERS READY = " +
            everyoneReady
        );
    }
}