using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.InputSystem;

public class LobbyReadySystem : NetworkBehaviour
{
    private TMP_Text statusText;
    private TMP_Text hintText;

    private NetworkVariable<bool> isReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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
        if (IsOwner)
        {
            LobbyStatusPanel lobbyUI = FindAnyObjectByType<LobbyStatusPanel>();

            if (lobbyUI != null)
            {
                statusText = lobbyUI.statusText;
                hintText = lobbyUI.hintText;

                UpdateUI(isReady.Value);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            ToggleReadyServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleReadyServerRpc()
    {
        isReady.Value = !isReady.Value;
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