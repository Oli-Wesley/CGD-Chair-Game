using UnityEngine;

public class ChairCamera : MonoBehaviour
{
    [Header("Target Settings")]
    // reference to office chair
    [SerializeField] private Transform target;
    // position behind the chair?
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
    // follow speed
    [SerializeField] private float followSpeed = 10f;

    [Header("Rotation Settings")]
    // rotation speed
    [SerializeField] private float rotationSpeed = 5f;
    // how fast the look responds to stick movement
    [SerializeField] private float lookSensitivity = 15f;
    private float yaw = 0f;
    private float pitch = 15f;

    // chair input actions reference
    private ChairInputActions inputActions;

    // for me - this is the init
    private void Awake()
    {
        // init the input actions asset class
        inputActions = new ChairInputActions();
    }

    private void OnEnable()
    {
        // turn on when camera is active
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // turn off when camera is inactive
        inputActions.Disable();
    }

    // runs after the physics update assuring movement happens first before camera adjustment
    private void LateUpdate()
    {
        if (target == null) return;

        // Read Vector2 input from the generated Input Actions asset (Look action)
        Vector2 lookInput = inputActions.ChairCamera.Look.ReadValue<Vector2>();

        // multiply by sensitivity for look reactiveness - NEEDS TWEEK
        float stickX = lookInput.x * lookSensitivity;
        float stickY = lookInput.y * lookSensitivity;

        yaw += stickX * rotationSpeed * Time.deltaTime;
        pitch -= stickY * rotationSpeed * Time.deltaTime;
        // clamp vertical angle so it doesnt flip over
        pitch = Mathf.Clamp(pitch, -10f, 60f);

        // calculate desired rotation and position behind the chair
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = target.position + rotation * offset;

        // interpolate camera position and look at the chair
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}