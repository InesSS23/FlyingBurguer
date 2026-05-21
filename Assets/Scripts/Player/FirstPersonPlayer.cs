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

    private CharacterController controller;

    void Start()
    {
        // vai buscar o character controller do player
        controller = GetComponent<CharacterController>();

        // garante que a camera fica no sitio certo quando o jogo começa
        if (playerCamera != null)
        {
            playerCamera.SetParent(transform);
            playerCamera.localPosition = cameraLocalPosition;
            playerCamera.localRotation = Quaternion.identity;
        }

        // prende o rato para olhar à volta
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // se estiver pausado, não mexe
        if (Time.timeScale == 0f)
            return;

        OlharComRato();
        MoverComTeclado();
    }

    private void OlharComRato()
    {
        if (playerCamera == null || Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // roda o corpo/player para os lados
        transform.Rotate(Vector3.up * mouseX);

        // roda a camera para cima e para baixo
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

        // se tiver character controller, usa isto para respeitar colisões
        if (controller != null)
        {
            controller.Move(direction * moveSpeed * Time.deltaTime);
        }
        else
        {
            // plano b, mas este atravessa colliders
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}