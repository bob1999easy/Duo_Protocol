using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float invulnerableTime = 0.5f; // after a hit, can't be hit again for this long

    private NetworkVariable<int> health = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private PlayerHealthUI ui;
    private float invulnerableUntil;

    public bool IsDead => health.Value <= 0;

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += OnHealthChanged;

        if (IsServer)
            health.Value = maxHealth;
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= OnHealthChanged;
    }

    private void Update()
    {
        // only the owner shows the health bar, find the UI in the scene
        // (after a scene change the old UI is destroyed, so this finds the new one)
        if (!IsOwner || ui != null)
            return;

        ui = FindAnyObjectByType<PlayerHealthUI>();

        if (ui != null)
            ui.SetHealth(health.Value, maxHealth);
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (IsOwner && ui != null)
            ui.SetHealth(newValue, maxHealth);
    }

    // called by the server only
    public void TakeDamage(int amount)
    {
        if (!IsServer || IsDead || Time.time < invulnerableUntil)
            return;

        invulnerableUntil = Time.time + invulnerableTime;

        health.Value = Mathf.Max(0, health.Value - amount);

        if (health.Value <= 0 && GameManager.Instance != null)
            GameManager.Instance.CheckAllPlayersDead();
    }

    // called by the server when a (new) game starts
    public void ResetPlayer(Vector3 position, Quaternion rotation, bool teleport)
    {
        if (!IsServer)
            return;

        health.Value = maxHealth;
        invulnerableUntil = 0f;

        if (teleport)
        {
            // the owner moves the player, so tell the owner's client to teleport
            ClientRpcParams rpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { OwnerClientId }
                }
            };

            TeleportClientRpc(position, rotation, rpcParams);
        }
    }

    [ClientRpc]
    private void TeleportClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams rpcParams = default)
    {
        // a CharacterController overrides transform.position unless it is disabled first
        CharacterController cc = GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        transform.SetPositionAndRotation(position, rotation);

        if (cc != null) cc.enabled = true;
    }
}