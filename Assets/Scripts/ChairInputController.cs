using UnityEngine;



// BUILD NOTES
/// <summary>
/// Could we have it so holding down both keys creates a boost affect like a boost bar that runs out?
/// This boost is when both keys are held down
/// like kicking off hard and rolling
/// The left/right should swing the chair a bit but not feel counterintuitive to forward movement
/// Cant work out how to get left/right push - offsetting the push point didnt work
/// </summary>
//


public class ChairInputController : MonoBehaviour
{
    private chairscript chairController;

    [Header("Camera Reference")]
    // main camera/player camera reference from inspector
    [SerializeField] private Transform cameraTransform;

    // chair input actions reference
    private ChairInputActions inputActions;

    // for me - this is the init
    private void Awake()
    {
        // find the chairscript on this GameObject
        chairController = GetComponent<chairscript>();

        // init the input actions asset class
        inputActions = new ChairInputActions();

        // find camera if not assigned - keep forgetting to assign!
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable()
    {
        // turn on when is active
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // turn off when is inactive
        inputActions.Disable();
    }

    private void Update()
    {
        // listen for individual foot inputs and standard push/space trigger from input mapping
        bool leftPressed = inputActions.ChairCamera.LeftFoot.IsPressed();
        bool rightPressed = inputActions.ChairCamera.RightFoot.IsPressed();
        bool standardPushTriggered = inputActions.ChairCamera.Push.triggered;

        if (chairController != null)
        {
            // Get the camera's forward direction, but flatten it on the Y-axis 
            // so we only push horizontally across the floor (no pushing into or up from the ground)
            // create a holder for the direction "push" direction
            Vector3 pushDirection = Vector3.forward;
            // check camera exists (debug issues)
            if (cameraTransform != null)
            {
                // get camera forward vector and assign
                pushDirection = cameraTransform.forward;
                // remove any tilt (maybe tilt on second stick? for look but not movement? would this need second camera?)
                pushDirection.y = 0f;
                // NORMALISE! :D
                pushDirection.Normalize();
            }
            else
            {
                // get chairs forward if camera is missing (debug issues)
                pushDirection = transform.forward;
            }

            // BOTH buttons pressed together (or standard push action/space) more powerful push from the center
            if ((leftPressed && rightPressed) || standardPushTriggered)
            {
                // force vector using the camera direction
                // higher magnitude for a powerful push
                Vector3 pushForce = pushDirection * 1.0f; 

                // use current position as the hit point
                Vector3 hitPoint = transform.position;

                // function on chair script
                chairController.PushChair(pushForce, hitPoint);
            }
            // LEFT push only applies force on the left side causing a rightward spin via torque
            else if (leftPressed)
            {
                Vector3 pushForce = pushDirection * 0.2f;
                // offset hit point left
                Vector3 hitPoint = transform.position - (transform.right * 1.5f);

                chairController.PushChair(pushForce, hitPoint);
            }
            // RIGHT push only applies force on the right side causing a leftward spin via torque
            else if (rightPressed)
            {
                Vector3 pushForce = pushDirection * 0.2f;
                // offset hit point right
                Vector3 hitPoint = transform.position + (transform.right * 1.5f);

                chairController.PushChair(pushForce, hitPoint);
            }
        }
    }
}