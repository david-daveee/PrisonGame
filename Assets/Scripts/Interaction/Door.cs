using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private AudioSource audioSource;

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

    public void Interact(PlayerInteractor interactor)
    {
        isMoving = true;
        isOpened = !isOpened;
        hasPlayedMovementSound = false;
    }

    public string GetInteractionText()
    {
        return isOpened ? "Close Door" : "Open Door";
    }
    private void Update()
    {
        UpdateDoorRotation();
    }

    private void UpdateDoorRotation()
    {
        float targetAngle = isOpened ? openAngle : 0f;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        doorPivot.localRotation = Quaternion.RotateTowards(
            doorPivot.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        TryPlayMovementSound();
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