using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(ContainerInventory))]
public class ContainerInteractable : MonoBehaviour, IInteractable
{
    [Header("Container")]
    [SerializeField] private ContainerInventory containerInventory;
    [SerializeField] private string interactionText;

    [Header("Optional Animator")]
    [Tooltip("Leave empty when this container has no animation.")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    [SerializeField] private string closeTrigger = "Close";

    [Header("Optional Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    [Header("Optional Events")]
    [SerializeField] private UnityEvent onOpened = new UnityEvent();
    [SerializeField] private UnityEvent onClosed = new UnityEvent();

    private bool isOpen;

    private void Reset()
    {
        containerInventory = GetComponent<ContainerInventory>();
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (containerInventory == null)
        {
            containerInventory = GetComponent<ContainerInventory>();
        }
    }

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
        if (isOpen)
        {
            return;
        }

        if (containerInventory == null)
        {
            Debug.LogError(
                "ContainerInteractable requires a ContainerInventory reference.",
                this
            );
            return;
        }

        isOpen = true;
        PlayAnimatorTrigger(openTrigger, closeTrigger);
        PlaySound(openSound);
        onOpened.Invoke();
        containerInventory.OpenFor(interactor);
    }

    public string GetInteractionText()
    {
        if (!string.IsNullOrWhiteSpace(interactionText))
        {
            return interactionText;
        }

        return containerInventory != null
            ? $"Open {containerInventory.ContainerName}"
            : "Open Container";
    }

    private void HandleContainerClosed()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        PlayAnimatorTrigger(closeTrigger, openTrigger);
        PlaySound(closeSound);
        onClosed.Invoke();
    }

    private void PlayAnimatorTrigger(
        string triggerToSet,
        string triggerToReset
    )
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerToSet))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(triggerToReset))
        {
            animator.ResetTrigger(triggerToReset);
        }

        animator.SetTrigger(triggerToSet);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
