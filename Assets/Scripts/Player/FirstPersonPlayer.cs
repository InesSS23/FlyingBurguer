using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonPlayer : MonoBehaviour
{
    [Header("movimento")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float sprintMultiplier = 1.45f;
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float gravity = -24f;
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("camera")]
    [SerializeField] private Transform playerCamera;

    [Header("posição da camera dentro do player")]
    [SerializeField] private Vector3 cameraLocalPosition = new Vector3(0f, 7.78f, 0f);

    [Header("posição inicial do jogador no Level1")]
    [SerializeField] private Vector3 startPosition = new Vector3(14.5f, 0f, 12.9f);
    [SerializeField] private Vector3 startRotation = new Vector3(0f, 244.9f, 0f);

    private float cameraPitch = 0f;
    private CharacterController characterController;
    private Vector3 planarVelocity;
    private float verticalVelocity;

    private bool canMove = true;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mouseSensitivity = GameSettings.GetMouseSensitivity();

        // CORREÇÃO: o Level1 passa a corrigir sozinho a posição inicial do jogador e da câmara.
        // Assim não depende do MainMenu nem da ordem em que o WebGL/itch.io carrega os scripts.
        ResetPlayerAndCameraToStart();
    }

    void Start()
    {
        LockCursor();

        // CORREÇÃO: força novamente durante os primeiros frames.
        // Isto evita que outro script ou a troca de cena altere a câmara logo no arranque.
        StartCoroutine(ForceCameraAtStart());
    }

    private void OnEnable()
    {
        // Quando o diálogo acaba, este script volta a ser ativado.
        // Nessa altura garantimos outra vez que a vista começa correta.
        ResetCameraView();
    }

    private IEnumerator ForceCameraAtStart()
    {
        ResetPlayerAndCameraToStart();

        yield return null;
        ResetPlayerAndCameraToStart();

        yield return null;
        ResetPlayerAndCameraToStart();
    }

    public void ResetPlayerAndCameraToStart()
    {
        if (characterController != null)
            characterController.enabled = false;

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(startRotation);

        ResetCameraView();

        if (characterController != null)
            characterController.enabled = true;
    }

    public void ResetCameraView()
    {
        cameraPitch = 0f;

        if (playerCamera != null)
        {
            playerCamera.SetParent(transform, false);
            playerCamera.localPosition = cameraLocalPosition;
            playerCamera.localRotation = Quaternion.identity;
        }
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

        bool sprinting = Keyboard.current.leftShiftKey.isPressed && direction.sqrMagnitude > 0f;
        float targetSpeed = moveSpeed * (sprinting ? sprintMultiplier : 1f);
        Vector3 targetVelocity = direction * targetSpeed;
        float velocityChange = direction.sqrMagnitude > 0f ? acceleration : deceleration;
        planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, velocityChange * Time.deltaTime);

        if (characterController != null)
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            characterController.Move((planarVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
        else
        {
            transform.position += planarVelocity * Time.deltaTime;
        }
    }
}
