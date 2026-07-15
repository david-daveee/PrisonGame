using UnityEngine;
using UnityEngine.UI;

public class InventoryCellUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;

    public Vector2Int Position { get; private set; }

    public void Initialize(Vector2Int position)
    {
        Position = position;
    }
}