using UnityEngine;
using Unity.Netcode;

public class EnemyAI : NetworkBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    // How often the enemy searches for the closest player
    [SerializeField] private int checkEveryFrames = 10;

    private int frameCounter = 0;
    private Transform targetPlayer;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

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

        float currentSpeed = 0f;

        // Move toward and rotate toward the selected - closest - player
        if (targetPlayer != null)
        {
            Vector3 direction = targetPlayer.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                // P³ynny obrót w stronê gracza
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                // Ruch w stronê gracza
                direction.Normalize();
                transform.position += direction * speed * Time.deltaTime;
                currentSpeed = speed;
            }
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
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