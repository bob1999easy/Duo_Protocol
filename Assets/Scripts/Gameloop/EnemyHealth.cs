using UnityEngine;
using Unity.Netcode;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;

    // synced to everyone so every client can draw the health bar
    private NetworkVariable<int> health = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public int MaxHealth => maxHealth;
    public int CurrentHealth => health.Value;

    // other scripts (health bar) can listen to changes
    public event System.Action<int, int> OnHealthChanged; // current, max

    public override void OnNetworkSpawn()
    {
        health.OnValueChanged += HandleHealthChanged;

        if (IsServer)
            health.Value = maxHealth;

        // clients that join / spawn late get the current value right away
        OnHealthChanged?.Invoke(health.Value, maxHealth);
    }

    public override void OnNetworkDespawn()
    {
        health.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldValue, int newValue)
    {
        OnHealthChanged?.Invoke(newValue, maxHealth);
    }

    // called by the server only
    public void TakeDamage(int amount)
    {
        if (!IsServer || health.Value <= 0)
            return;

        health.Value = Mathf.Max(0, health.Value - amount);

        if (health.Value <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EnemyDied();

            NetworkObject.Despawn(true);
        }
    }
}