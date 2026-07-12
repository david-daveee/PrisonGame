using UnityEngine;

public class PlayerUIStateController : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCameraController playerCameraController;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private GameObject gameplayUIRoot;

    private bool isInUIMode;

    public void EnterUIMode()
    {
        if (isInUIMode)
        {
            return;
        }

        isInUIMode = true;

        playerController.enabled = false;
        playerCameraController.enabled = false;
        playerInteractor.enabled = false;

        gameplayUIRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitUIMode()
    {
        if (!isInUIMode)
        {
            return;
        }

        isInUIMode = false;

        playerController.enabled = true;
        playerCameraController.enabled = true;
        playerInteractor.enabled = true;

        gameplayUIRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsInUIMode()
    {
        return isInUIMode;
    }
}