using UnityEngine;

public class PlayerInventoryInput : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private PlayerUIStateController playerUIStateController;

    private bool isInventoryOpen;

    private void Update()
    {
        if (playerInputHandler.InventoryPressedThisFrame())
        {
            ToggleInventory();
        }

        if (isInventoryOpen &&
            playerInputHandler.RotateInventoryItemPressedThisFrame())
        {
            inventoryUI.RotateHoveredItem();
        }
    }

    private void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            inventoryUI.Close();
            playerUIStateController.ExitUIMode();

            isInventoryOpen = false;
            return;
        }

        inventoryUI.Open(playerInventory);
        playerUIStateController.EnterUIMode();

        isInventoryOpen = true;
    }

    public void OpenContainer(ContainerInventory container)
    {
        if (container == null || isInventoryOpen)
        {
            return;
        }

        inventoryUI.OpenContainer(playerInventory, container);
        playerUIStateController.EnterUIMode();
        isInventoryOpen = true;
    }
}
