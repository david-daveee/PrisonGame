using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text actionText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        root.SetActive(true);
        keyText.text = "[E]";
        actionText.text = text;
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}