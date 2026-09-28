using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // for "P" key while changing scenes

public class LobbyReadySystem : NetworkBehaviour
{
    private TMP_Text statusText;
    private TMP_Text hintText;

    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IsReady => isReady.Value;

    private void OnEnable()
    {
        isReady.OnValueChanged += OnReadyStateChanged;
    }

    private void OnDisable()
    {
        isReady.OnValueChanged -= OnReadyStateChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        LobbyStatusPanel lobbyUI =
            FindAnyObjectByType<LobbyStatusPanel>();

        if (lobbyUI != null)
        {
            statusText = lobbyUI.statusText;
            hintText = lobbyUI.hintText;

            UpdateUI(isReady.Value);
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        // disable ability to press "P" after changing to the GameScene from MainScene - so it won't show it in the console 
        if (SceneManager.GetActiveScene().name != "MainScene")
            return;

        if (Keyboard.current != null &&
            Keyboard.current.pKey.wasPressedThisFrame)
        {
            ToggleReadyServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleReadyServerRpc()
    {
        isReady.Value = !isReady.Value;

        Debug.Log(
            "Player " +
            OwnerClientId +
            " READY = " +
            isReady.Value
        );

        LobbyManager lobbyManager =
            FindAnyObjectByType<LobbyManager>();

        if (lobbyManager != null)
        {
            lobbyManager.CheckAllPlayersReady();
        }
    }

    private void OnReadyStateChanged(bool oldValue, bool newValue)
    {
        if (!IsOwner)
            return;

        UpdateUI(newValue);
    }

    private void UpdateUI(bool ready)
    {
        if (statusText == null || hintText == null)
            return;

        if (ready)
        {
            statusText.text = "READY";
            hintText.text = "Press [P] to Unready";
        }
        else
        {
            statusText.text = "NOT READY";
            hintText.text = "Press [P] to Ready";
        }
    }
}