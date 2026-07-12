using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform cameraPivot;

    private float verticalRotation;

    private void Update()
    {
        Vector2 lookInput = ReadLookInput();

        RotateHorizontal(lookInput);
        RotateVertical(lookInput);
    }

    private Vector2 ReadLookInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        return new Vector2(mouseX, mouseY);
    }

    private void RotateHorizontal(Vector2 lookInput)
    {
        playerBody.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    private void RotateVertical(Vector2 lookInput)
    {
        verticalRotation -= lookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);

        cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
}