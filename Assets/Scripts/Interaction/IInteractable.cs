public interface IInteractable
{
    void Interact(PlayerInteractor interactor);

    string GetInteractionText();
}