using UnityEngine;

public class Vent : MonoBehaviour, IInteractable
{
    private enum VentState
    {
        Closed,
        MovingToDrop,
        MovingToRemoved,
        Opened,
        MovingBackToDrop,
        MovingToClosed
    }

    [SerializeField] private Transform cover;
    [SerializeField] private Transform closedPoint;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private Transform removedPoint;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float reachDistance = 0.01f;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clangSound;
    [SerializeField] private AudioClip attachSound;

    [SerializeField] private Bolt[] bolts;

    private VentState currentState = VentState.Closed;

    public void Interact(PlayerInteractor interactor)
    {
        switch (currentState)
        {
            case VentState.Closed:
                if (!AreAllBoltsRemoved())
                {
                    return;
                }
                currentState = VentState.MovingToDrop;
                break;

            case VentState.Opened:
                currentState = VentState.MovingBackToDrop;
                break;
        }
    }

    public string GetInteractionText()
    {
        switch (currentState)
        {
            case VentState.Closed:
                if (!AreAllBoltsRemoved())
                {
                    return "Unscrew all bolts first";
                }

                return "Open Vent";

            case VentState.Opened:
                return "Close Vent";

            default:
                return "Moving...";
        }
    }

    private void Update()
    {
        UpdateCoverMovement();
    }

    private void UpdateCoverMovement()
    {
        switch (currentState)
        {
            case VentState.MovingToDrop:
                if (MoveCoverTo(dropPoint))
                {
                    PlaySound(clangSound);
                    currentState = VentState.MovingToRemoved;
                }
                break;

            case VentState.MovingToRemoved:
                if (MoveCoverTo(removedPoint))
                {

                    currentState = VentState.Opened;
                }
                break;

            case VentState.MovingBackToDrop:
                if (MoveCoverTo(dropPoint))
                {
                    PlaySound(attachSound);
                    currentState = VentState.MovingToClosed;
                }
                break;

            case VentState.MovingToClosed:
                if (MoveCoverTo(closedPoint))
                {

                    currentState = VentState.Closed;
                }
                break;
        }
    }

    private bool MoveCoverTo(Transform targetPoint)
    {
        cover.position = Vector3.MoveTowards(
            cover.position,
            targetPoint.position,
            moveSpeed * Time.deltaTime
        );

        float distance = Vector3.Distance(cover.position, targetPoint.position);

        return distance <= reachDistance;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    private bool AreAllBoltsRemoved()
    {
        foreach (Bolt bolt in bolts)
        {
            if (!bolt.IsRemoved())
            {
                return false;
            }
        }

        return true;
    }
}