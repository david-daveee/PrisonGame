using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    [SerializeField, Min(1)]
    private int initialAmount = 1;

    private InventoryItem inventoryItem;

    private void Awake()
    {
        if (itemData == null)
        {
            Debug.LogError(
                $"WorldItem '{name}' has no ItemData assigned.",
                this
            );

            enabled = false;
            return;
        }

        inventoryItem = new InventoryItem(itemData, initialAmount);
    }

    public void Interact(PlayerInteractor interactor)
    {
        PlayerInventory playerInventory = interactor.GetInventory();

        if (playerInventory == null || inventoryItem == null)
        {
            return;
        }

        if (playerInventory.TryAddItem(inventoryItem))
        {
            gameObject.SetActive(false);
        }
    }

    public string GetInteractionText()
    {
        if (itemData == null)
        {
            return string.Empty;
        }

        return $"Pick up {itemData.DisplayName}";
    }
}