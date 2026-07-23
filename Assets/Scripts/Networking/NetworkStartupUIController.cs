using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkStartupUIController : MonoBehaviour
{
    [SerializeField] private GameObject networkHudRoot;
    [SerializeField] private PlayerUIStateController playerUIStateController;

    private bool wasNetworkActive;
    private bool isHudVisible;

    private void Start()
    {
        if (networkHudRoot == null || playerUIStateController == null)
        {
            Debug.LogError(
                $"NetworkStartupUIController on '{name}' has missing references.",
                this
            );
            enabled = false;
            return;
        }

        wasNetworkActive = IsNetworkActive();
        SetHudVisible(!wasNetworkActive);
    }

    private void Update()
    {
        bool isNetworkActive = IsNetworkActive();

        if (isNetworkActive != wasNetworkActive)
        {
            SetHudVisible(!isNetworkActive);
            wasNetworkActive = isNetworkActive;
        }

        if (Keyboard.current?.f1Key.wasPressedThisFrame == true)
        {
            SetHudVisible(!isHudVisible);
        }
    }

    private void SetHudVisible(bool visible)
    {
        if (isHudVisible == visible && networkHudRoot.activeSelf == visible)
        {
            return;
        }

        isHudVisible = visible;
        networkHudRoot.SetActive(visible);

        if (visible)
        {
            playerUIStateController.EnterUIMode();
        }
        else
        {
            playerUIStateController.ExitUIMode();
        }
    }

    private static bool IsNetworkActive()
    {
        return NetworkServer.active || NetworkClient.active;
    }

    private void OnValidate()
    {
        if (networkHudRoot == null || playerUIStateController == null)
        {
            Debug.LogWarning(
                $"NetworkStartupUIController on '{name}' needs a HUD root " +
                "and PlayerUIStateController.",
                this
            );
        }
    }
}
