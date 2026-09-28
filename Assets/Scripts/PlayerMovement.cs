using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{

    private float speed = 5f;        // speed of the player
    private Camera playerCamera;    // camera assign with the player prefab

    private float mouseSensitivity = 0.3f; // speed of mouse rotation
    private float verticalRotation = 0f; // save angle of camera


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
        // movement
        if (!IsOwner) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        transform.Translate(direction * speed * Time.deltaTime);


        // mouse rotation
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            // left / right
            transform.Rotate(Vector3.up * mouseX);

            // up / down
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }
}
