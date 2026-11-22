using UnityEngine;
using Unity.Netcode;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;

    private int health; // for the server 

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            health = maxHealth;
    }

    // called by server only
    public void TakeDamage(int amount)
    {
        if (!IsServer || health <= 0)
            return;

        health -= amount;

        if (health <= 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.EnemyDied();

            NetworkObject.Despawn(true); // removes the enemy for everyone
        }
    }
}