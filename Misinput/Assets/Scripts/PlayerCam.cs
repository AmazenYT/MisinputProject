using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public Transform orientation;

    [SerializeField] private float startingYRotation = 0f;
    private bool mouseLookEnabled = false;

    float xRotation;
    float yRotation;

    private void Start()
    {
        // Start with mouse look disabled
        SetMouseLookState(false);

        // Initialize rotations
        yRotation = startingYRotation;
        xRotation = 0f;
        ApplyRotations();
    }

    public void SetMouseLookState(bool state)
    {
        mouseLookEnabled = state;
        Cursor.lockState = state ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !state;
    }

    private void Update()
    {
        if (!mouseLookEnabled) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        ApplyRotations();
    }

    private void ApplyRotations()
    {
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}