using UnityEngine;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float speed = 2f;

    // How often the enemy searches for the closest player
    [SerializeField] private int checkEveryFrames = 10;

    private int frameCounter = 0;

    private Transform targetPlayer;


    private void Update()
    {
        // only server controls the enemy
        if (!IsServer)
            return;

        frameCounter++;

        // Check players only every X frames
        if (frameCounter >= checkEveryFrames)
        {
            frameCounter = 0;

            FindClosestPlayer();
        }

        // Move toward the selected - closest -  player
        if (targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.position - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;
        }
    }


    private void FindClosestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject == null)
                continue;

            Transform playerTransform = client.PlayerObject.transform;

            float distance = (playerTransform.position - transform.position).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = playerTransform;
            }
        }

        targetPlayer = closestPlayer;

        if (targetPlayer != null)
        {
            Debug.Log("Enemy target = " + targetPlayer.name);
        }
    }
}