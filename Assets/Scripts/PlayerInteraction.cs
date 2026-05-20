using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("interação")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;

    void Start()
    {
        // se eu n meter a camera no inspector, ele tenta apanhar a main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        // clique esquerdo do rato
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null)
            return;

        // raio que sai do centro da camera para a frente
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // ve se o objeto clicado tem algum script interativo
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.Log("olhei para isto mas n é interativo: " + hit.collider.name);
            }
        }
    }
}