using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    public bool InventoryPressedThisFrame()
    {
        return inputActions.Player.Inventory.WasPressedThisFrame();
    }

    public bool RotateInventoryItemPressedThisFrame()
    {
        return inputActions.Player.RotateInventoryItem.WasPressedThisFrame();
    }
}
