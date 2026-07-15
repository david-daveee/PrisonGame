using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private InteractionUI interactionUI;
    private IInteractable currentInteractable;

    private void Update()
    {
        FindInteractable();

        if (ReadInteractInput())
        {
            Interact();
        }
    }

    private bool ReadInteractInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    private void FindInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactDistance))
        {
            currentInteractable = hitInfo.collider.GetComponentInParent<IInteractable>();

            if (currentInteractable != null)
            {
                interactionUI.Show(currentInteractable.GetInteractionText());
            }
            else
            {
                interactionUI.Hide();
            }
        }
        else
        {
            currentInteractable = null;
            interactionUI.Hide();
        }
    }

    private void Interact()
    {
        if (currentInteractable == null)
        {
            return;
        }

        currentInteractable.Interact(this);
    }

    public PlayerInventory GetInventory()
    {
        return GetComponent<PlayerInventory>();
    }

    public PlayerInventoryInput GetInventoryInput()
    {
        return GetComponent<PlayerInventoryInput>();
    }
}
