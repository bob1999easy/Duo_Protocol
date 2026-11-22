using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerShoot : NetworkBehaviour
{
    [Header("Shooting")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 0.2f; // time between shots

    [Header("Visuals")]
    [SerializeField] private LineRenderer tracer;           // child object with a LineRenderer
    [SerializeField] private float tracerDuration = 0.05f;  // how long the line stays visible
    [SerializeField] private GameObject hitEffectPrefab;    // particle spawned at the hit point
    [SerializeField] private Vector3 muzzleOffset = new Vector3(0.3f, -0.25f, 0.5f); // right, down, forward from the camera

    private Camera playerCamera;
    private float nextShotTime;
    private float tracerHideTime;

    private PlayerHealth playerHealth;

    public override void OnNetworkSpawn()
    {
        playerCamera = GetComponentInChildren<Camera>();
        playerHealth = GetComponent<PlayerHealth>();

        if (tracer != null)
            tracer.enabled = false;
    }

    private void Update()
    {
        // hide the tracer after a short time (runs for every player, owner or not)
        if (tracer != null && tracer.enabled && Time.time >= tracerHideTime)
            tracer.enabled = false;

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

        // basic cheat check - the shot must start near the player
        if ((origin - transform.position).sqrMagnitude > 9f)
            return;

        direction = direction.normalized;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction,
            range,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // if nothing is hit, the line goes to the maximum range
        Vector3 endPoint = origin + direction * range;
        Vector3 hitNormal = Vector3.up;
        bool hitSomething = false;

        foreach (RaycastHit hit in hits)
        {
            // ignore our own colliders
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            endPoint = hit.point;
            hitNormal = hit.normal;
            hitSomething = true;

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
                enemy.TakeDamage(damage);

            break; // first thing hit stops the bullet - wall, enemy, other player
        }

        // where the visible line starts (a bit to the right / below the camera)
        Vector3 startPoint = origin + Quaternion.LookRotation(direction) * muzzleOffset;

        ShootEffectClientRpc(startPoint, endPoint, hitNormal, hitSomething);
    }

    // runs on every client (and the host) - visuals only
    [ClientRpc]
    private void ShootEffectClientRpc(Vector3 start, Vector3 end, Vector3 normal, bool hitSomething)
    {
        if (tracer != null)
        {
            tracer.SetPosition(0, start);
            tracer.SetPosition(1, end);
            tracer.enabled = true;
            tracerHideTime = Time.time + tracerDuration;
        }

        if (hitSomething && hitEffectPrefab != null)
        {
            GameObject fx = Instantiate(
                hitEffectPrefab,
                end,
                Quaternion.LookRotation(normal)
            );

            Destroy(fx, 2f);
        }
    }
}