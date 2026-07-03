using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("interacao")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 10f;
    [SerializeField] private bool debugRaycastHits = false;

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
            Debug.Log("nao tenho camera para interagir");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, ~0, QueryTriggerInteraction.Collide);

        if (hits.Length == 0)
        {
            Debug.Log("nao acertei em nada com o clique");
            return;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            if (debugRaycastHits)
            {
                Debug.Log("raycast hit: " + hit.collider.name);
            }

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (interactable != null)
            {
                Debug.Log("interagi com: " + hit.collider.name);
                interactable.Interact();
                return;
            }
        }

        Debug.Log("olhei para objetos, mas nenhum tinha interacao");
    }
}
