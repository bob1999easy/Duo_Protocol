using UnityEngine;
using Unity.Netcode;

public class EnemyAttack : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;   // distance at which the enemy hits
    [SerializeField] private float attackCooldown = 1f;  // seconds between this enemy's hits

    private float nextAttackTime;

    private void Update()
    {
        // only the server handles damage
        if (!IsServer)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (Time.time < nextAttackTime)
            return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            PlayerHealth playerHealth = client.PlayerObject.GetComponent<PlayerHealth>();

            if (playerHealth == null || playerHealth.IsDead)
                continue;

            float sqrDistance =
                (client.PlayerObject.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance <= attackRange * attackRange)
            {
                playerHealth.TakeDamage(damage);
                nextAttackTime = Time.time + attackCooldown;
                break; // one hit per attack
            }
        }
    }
}