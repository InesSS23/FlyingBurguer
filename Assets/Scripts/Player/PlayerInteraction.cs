using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("interação")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 10f;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.Log("n tenho camera para interagir");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, ~0, QueryTriggerInteraction.Collide))
        {
            Debug.Log("cliquei/olhei para: " + hit.collider.name);

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.Log("este objeto n tem interação: " + hit.collider.name);
            }
        }
        else
        {
            Debug.Log("n acertei em nada com o clique");
        }
    }
}