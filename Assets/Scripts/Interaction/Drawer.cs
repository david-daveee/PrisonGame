using UnityEngine;

public class Drawer : MonoBehaviour, IInteractable
{
    private enum DrawerState
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

    private DrawerState currentState = DrawerState.Closed;

    public void Interact(PlayerInteractor interactor)
    {
        switch (currentState)
        {
            case DrawerState.Closed:
                if (!AreAllBoltsRemoved())
                {
                    return;
                }
                currentState = DrawerState.MovingToDrop;
                break;

            case DrawerState.Opened:
                currentState = DrawerState.MovingBackToDrop;
                break;
        }
    }

    public string GetInteractionText()
    {
        switch (currentState)
        {
            case DrawerState.Closed:
                if (!AreAllBoltsRemoved())
                {
                    return "Unscrew all bolts first";
                }

                return "Open Drawer";

            case DrawerState.Opened:
                return "Close Drawer";

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
            case DrawerState.MovingToDrop:
                if (MoveCoverTo(dropPoint))
                {
                    PlaySound(clangSound);
                    currentState = DrawerState.MovingToRemoved;
                }
                break;

            case DrawerState.MovingToRemoved:
                if (MoveCoverTo(removedPoint))
                {

                    currentState = DrawerState.Opened;
                }
                break;

            case DrawerState.MovingBackToDrop:
                if (MoveCoverTo(dropPoint))
                {
                    PlaySound(attachSound);
                    currentState = DrawerState.MovingToClosed;
                }
                break;

            case DrawerState.MovingToClosed:
                if (MoveCoverTo(closedPoint))
                {

                    currentState = DrawerState.Closed;
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