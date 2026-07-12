using System.Collections;
using UnityEngine;

public class Bolt : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject visual;
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip unscrewSound;

    [Header("Animation")]
    [SerializeField] private float removeDuration = 1f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private Vector3 removeOffset = new Vector3(0f, 0f, 0.05f);

    private bool isRemoved;
    private bool isRemoving;

    private void Awake()
    {

    }

    public void Interact(PlayerInteractor interactor)
{
    if (isRemoved || isRemoving)
    {
        return;
    }

    PlayerInventory playerInventory = interactor.GetInventory();

    if (playerInventory == null)
    {
        return;
    }

    if (!playerInventory.HasItem(ItemId.Screwdriver))
    {
        return;
    }

    StartCoroutine(RemoveBoltRoutine());
}

    public string GetInteractionText()
    {
        if (isRemoving)
        {
            return "Unscrewing...";
        }

        return "Unscrew Bolt";
    }

    public bool IsRemoved()
    {
        return isRemoved;
    }

    private IEnumerator RemoveBoltRoutine()
    {
        isRemoving = true;
        interactionCollider.enabled = false;

        PlaySound(unscrewSound);

        Vector3 startPosition = visual.transform.localPosition;
        Vector3 targetPosition = startPosition + removeOffset;

        float elapsedTime = 0f;

        while (elapsedTime < removeDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / removeDuration;

            visual.transform.localPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );

            visual.transform.Rotate(
                Vector3.forward,
                rotationSpeed * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        isRemoved = true;
        isRemoving = false;

        visual.SetActive(false);
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