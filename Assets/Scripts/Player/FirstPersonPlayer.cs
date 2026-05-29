using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonPlayer : MonoBehaviour
{
    [Header("movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("camera")]
    [SerializeField] private Transform playerCamera;

    [Header("posição da camera dentro do player")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 7.78f, 0f);

    private float cameraPitch = 0f;
    private CharacterController characterController;

    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        mouseSensitivity = GameSettings.GetMouseSensitivity();

        if (playerCamera != null)
        {
            playerCamera.SetParent(transform);
            playerCamera.localPosition = cameraLocalPosition;
            playerCamera.localRotation = Quaternion.identity;
        }

        LockCursor();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (!canMove)
            return;

        OlharComRato();
        MoverComTeclado();
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OlharComRato()
    {
        if (playerCamera == null || Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void MoverComTeclado()
    {
        if (Keyboard.current == null)
            return;

        Vector3 direction = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            direction += transform.forward;

        if (Keyboard.current.sKey.isPressed)
            direction -= transform.forward;

        if (Keyboard.current.dKey.isPressed)
            direction += transform.right;

        if (Keyboard.current.aKey.isPressed)
            direction -= transform.right;

        direction.y = 0f;
        direction.Normalize();

        if (characterController != null)
        {
            characterController.Move(direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}