using UnityEngine;

public class InventoryDebug : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Test Items")]
    [SerializeField] private ItemData[] startItems;

    private void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError(
                "InventoryDebug has no PlayerInventory assigned.",
                this
            );
            enabled = false;
            return;
        }

        foreach (ItemData itemData in startItems)
        {
            if (itemData == null)
            {
                continue;
            }

            InventoryItem item = new InventoryItem(itemData, 1);

            if (!playerInventory.TryAddItem(item))
            {
                Debug.LogWarning(
                    $"No room for {itemData.DisplayName}.",
                    this
                );
            }
        }
    }
}
