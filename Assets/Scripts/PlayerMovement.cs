using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{

    public float speed = 5f;        // speed of the player
    private Camera playerCamera;    // camera assign with the player prefab

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();            // at the start find the camera component in the prefab

        if (IsOwner)
        {
            GetComponent<Renderer>().material.color = Color.red;    // if you are the owner - you are the player - assign yourself color red (you won't see it)

            playerCamera.enabled = true;                            // enable that camera in your prefab
        }
        else
        {
            playerCamera.enabled = false;                           // disable every other camera - in prefabs of other players
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
