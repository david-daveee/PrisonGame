using UnityEngine;
using UnityEngine.UI;

public class InventoryCellUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;

    private Color baseColor;

    public Vector2Int Position { get; private set; }

    public void Initialize(Vector2Int position)
    {
        Position = position;

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    public void SetBaseColor(Color color)
    {
        baseColor = color;
        backgroundImage.color = baseColor;
    }

    public void ShowPlacementPreview(Color color)
    {
        backgroundImage.color = color;
    }

    public void ClearPlacementPreview()
    {
        backgroundImage.color = baseColor;
    }
}
