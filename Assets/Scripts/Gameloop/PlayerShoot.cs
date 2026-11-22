using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerShoot : NetworkBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 0.2f; // time between shots

    private Camera playerCamera;
    private float nextShotTime;

    public override void OnNetworkSpawn()
    {
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (!IsOwner || playerCamera == null)
            return;

        // no shooting in the lobby / after the game is over
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        if (Mouse.current != null &&
            Mouse.current.leftButton.isPressed &&
            Time.time >= nextShotTime)
        {
            nextShotTime = Time.time + fireRate;

            ShootServerRpc(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 origin, Vector3 direction)
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
            return;

        // basic cheat check - that the shot must start near the player
        if ((origin - transform.position).sqrMagnitude > 9f)
            return;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction.normalized,
            range,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // ignore our own colliders
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
                enemy.TakeDamage(damage);

            break; // first thing hit stops the bullet - wall, enemy, other player
        }
    }
}