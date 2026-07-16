using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    [SerializeField, Min(1)]
    private int initialAmount = 1;

    [SerializeField] private SpriteRenderer iconRenderer;

    private InventoryItem inventoryItem;

    private void Awake()
    {
        if (itemData != null)
        {
            ApplyPresentation();
        }
    }

    private void Start()
    {
        if (inventoryItem != null)
        {
            return;
        }

        if (itemData == null)
        {
            Debug.LogError(
                $"WorldItem '{name}' was not initialized and has no ItemData.",
                this
            );
            enabled = false;
            return;
        }

        int amount = Mathf.Clamp(initialAmount, 1, itemData.MaxStack);
        inventoryItem = new InventoryItem(itemData, amount);
    }

    public bool Initialize(InventoryItem item)
    {
        if (item == null || item.ItemData == null)
        {
            Debug.LogError(
                $"WorldItem '{name}' received an invalid InventoryItem.",
                this
            );
            return false;
        }

        inventoryItem = item;
        itemData = item.ItemData;
        initialAmount = item.Amount;
        enabled = true;
        ApplyPresentation();
        return true;
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
            Destroy(gameObject);
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

    private void ApplyPresentation()
    {
        if (iconRenderer != null)
        {
            iconRenderer.sprite = itemData.Icon;
            iconRenderer.enabled = itemData.Icon != null;
        }

        name = $"WorldItem_{itemData.DisplayName}";
    }
}
