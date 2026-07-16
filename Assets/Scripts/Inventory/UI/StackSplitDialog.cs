using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StackSplitDialog : MonoBehaviour
{
    [SerializeField] private Slider amountSlider;
    [SerializeField] private TMP_InputField amountInput;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;

    private Action<int> applyAction;
    private Action cancelAction;
    private int minimumAmount;
    private int maximumAmount;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        amountSlider.wholeNumbers = true;
        amountSlider.onValueChanged.AddListener(HandleSliderChanged);
        amountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        amountInput.onEndEdit.AddListener(HandleInputChanged);
        applyButton.onClick.AddListener(Apply);
        cancelButton.onClick.AddListener(Cancel);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            Cancel();
        }
    }

    public bool Open(
        int minimum,
        int maximum,
        Action<int> onApply,
        Action onCancel)
    {
        if (!enabled || maximum < minimum)
        {
            return false;
        }

        minimumAmount = minimum;
        maximumAmount = maximum;
        applyAction = onApply;
        cancelAction = onCancel;

        amountSlider.minValue = minimumAmount;
        amountSlider.maxValue = maximumAmount;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        SetAmount(minimumAmount);
        amountInput.Select();
        amountInput.ActivateInputField();
        return true;
    }

    public void Cancel()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        Action callback = cancelAction;
        CloseInternal();
        callback?.Invoke();
    }

    public void CloseAfterApply()
    {
        CloseInternal();
    }

    private void Apply()
    {
        int amount = Mathf.Clamp(
            Mathf.RoundToInt(amountSlider.value),
            minimumAmount,
            maximumAmount
        );
        applyAction?.Invoke(amount);
    }

    private void HandleSliderChanged(float value)
    {
        SetAmount(Mathf.RoundToInt(value));
    }

    private void HandleInputChanged(string value)
    {
        if (!int.TryParse(value, out int parsedAmount))
        {
            SetAmount(Mathf.RoundToInt(amountSlider.value));
            return;
        }

        SetAmount(parsedAmount);
    }

    private void SetAmount(int amount)
    {
        int clampedAmount = Mathf.Clamp(
            amount,
            minimumAmount,
            maximumAmount
        );
        amountSlider.SetValueWithoutNotify(clampedAmount);
        amountInput.SetTextWithoutNotify(clampedAmount.ToString());
        amountText.text = $"x{clampedAmount}";
    }

    private void CloseInternal()
    {
        applyAction = null;
        cancelAction = null;
        gameObject.SetActive(false);
    }

    private bool ValidateReferences()
    {
        bool valid = amountSlider != null &&
                     amountInput != null &&
                     amountText != null &&
                     applyButton != null &&
                     cancelButton != null;

        if (!valid)
        {
            Debug.LogError(
                $"StackSplitDialog prefab '{name}' has missing references.",
                this
            );
        }

        return valid;
    }

    private void OnValidate()
    {
        ValidateReferences();
    }
}
