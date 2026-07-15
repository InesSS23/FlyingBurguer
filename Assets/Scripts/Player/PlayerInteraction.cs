using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("interacao")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 10f;
    [SerializeField] private bool debugRaycastHits = false;

    private IInteractable focusedInteractable;
    private Component focusedComponent;

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        focusedInteractable = FindInteractable(out focusedComponent);

        if (GameplayHUDPolish.Instance != null)
            GameplayHUDPolish.Instance.SetInteractionTarget(focusedComponent);

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

        IInteractable interactable = focusedInteractable ?? FindInteractable(out focusedComponent);

        if (interactable == null)
        {
            if (GameplayHUDPolish.Instance != null)
                GameplayHUDPolish.Instance.PulseInteraction(false);
            return;
        }

        interactable.Interact();

        if (GameplayHUDPolish.Instance != null)
            GameplayHUDPolish.Instance.PulseInteraction(true);
    }

    private IInteractable FindInteractable(out Component interactableComponent)
    {
        interactableComponent = null;

        if (playerCamera == null)
            return null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance, ~0, QueryTriggerInteraction.Collide);

        if (hits.Length == 0)
            return null;

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
                interactableComponent = interactable as Component;
                return interactable;
            }
        }

        return null;
    }
}
