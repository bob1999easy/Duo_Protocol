using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    // basic movement
    private float speed = 5f;        // speed of the player
    private Camera playerCamera;    // camera assign with the player prefab
    private Animator animator;      // animator component on the model

    private float mouseSensitivity = 0.3f; // speed of mouse rotation
    private float verticalRotation = 0f; // save angle of camera

    // jump
    private CharacterController characterController;
    private float verticalVelocity = 0f;

    private float jumpHeight = 1.5f;
    private float gravity = -20f;

    // network variable for synchronizing animation speed
    private NetworkVariable<float> networkSpeed = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();            // at the start find the camera component in the prefab
        animator = GetComponentInChildren<Animator>();              // find the animator on the model child
        characterController = GetComponent<CharacterController>();  // find character controller

        if (IsOwner)
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null) rend.material.color = Color.red;      // if you are the owner assign color red

            if (playerCamera != null) playerCamera.enabled = true;  // enable that camera in your prefab

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (playerCamera != null) playerCamera.enabled = false; // disable camera on remote players
        }
    }

    // Update is called once per frame
    void Update()
    {
        // MOVEMENT & INPUT (Owner only)
        if (IsOwner)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            Vector3 move = transform.TransformDirection(direction) * speed;

            // JUMP + GRAVITY
            if (characterController != null)
            {
                if (characterController.isGrounded && verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }

                if (characterController.isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                verticalVelocity += gravity * Time.deltaTime;
                move.y = verticalVelocity;

                // Move player
                characterController.Move(move * Time.deltaTime);
            }

            // Sync animation speed (0 when standing, 1 when moving)
            networkSpeed.Value = direction.magnitude;

            // MOUSE ROTATION
            if (Mouse.current != null)
            {
                Vector2 mouseDelta = Mouse.current.delta.ReadValue();

                float mouseX = mouseDelta.x * mouseSensitivity;
                float mouseY = mouseDelta.y * mouseSensitivity;

                // left / right
                transform.Rotate(Vector3.up * mouseX);

                // up / down
                if (playerCamera != null)
                {
                    verticalRotation -= mouseY;
                    verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
                    playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
                }
            }
        }

        // ANIMATION UPDATE (Runs for everyone, owner and clients)
        if (animator != null)
        {
            animator.SetFloat("Speed", networkSpeed.Value, 0.1f, Time.deltaTime);
        }
    }
}