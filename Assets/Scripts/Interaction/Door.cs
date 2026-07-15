using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private AudioSource audioSource;

    [Header("Inventory (Optional)")]
    [Tooltip("Assign a container when this door should open inventory UI.")]
    [SerializeField] private ContainerInventory containerInventory;

    [Header("Rotation")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private float soundTriggerAngle = 5f;

    private bool isOpened;
    private bool isMoving;
    private bool hasPlayedMovementSound;

    private void OnEnable()
    {
        if (containerInventory != null)
        {
            containerInventory.Closed += HandleContainerClosed;
        }
    }

    private void OnDisable()
    {
        if (containerInventory != null)
        {
            containerInventory.Closed -= HandleContainerClosed;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        isMoving = true;
        isOpened = !isOpened;
        hasPlayedMovementSound = false;

        if (isOpened)
        {
            containerInventory?.OpenFor(interactor);
        }
    }

    public string GetInteractionText()
    {
        if (!isOpened && containerInventory != null)
        {
            return $"Open {containerInventory.ContainerName}";
        }

        return isOpened ? "Close Door" : "Open Door";
    }
    private void Update()
    {
        UpdateDoorRotation();
    }

    private void UpdateDoorRotation()
    {
        if (doorPivot == null)
        {
            return;
        }

        float targetAngle = isOpened ? openAngle : 0f;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        doorPivot.localRotation = Quaternion.RotateTowards(
            doorPivot.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        TryPlayMovementSound();

        if (Quaternion.Angle(doorPivot.localRotation, targetRotation) < 0.01f)
        {
            isMoving = false;
        }
    }

    private void HandleContainerClosed()
    {
        if (!isOpened)
        {
            return;
        }

        isOpened = false;
        isMoving = true;
        hasPlayedMovementSound = false;
    }

    private void TryPlayMovementSound()
    {
        if (!isMoving)
        {
            return;
        }
        if (hasPlayedMovementSound)
        {
            return;
        }

        if (audioSource == null)
        {
            return;
        }

        float currentAngle = doorPivot.localEulerAngles.y;

        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        if (isOpened && Mathf.Abs(currentAngle) >= soundTriggerAngle)
        {
            PlaySound(openSound);
        }
        else if (!isOpened && Mathf.Abs(currentAngle) <= Mathf.Abs(openAngle) - soundTriggerAngle)
        {
            PlaySound(closeSound);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
        hasPlayedMovementSound = true;
    }
}
