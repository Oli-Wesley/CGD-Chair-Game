using UnityEngine;

public class chairscript : MonoBehaviour
{
    // define maximum speed
    [SerializeField] private float maxSpeed = 20f;

    // array of wheels
    private wheelscript[] wheels_array;
    // reference to ridged body for the chair
    private Rigidbody chair_rb;
    // profile for all wheels
    private WheelProfile sharedProfile;
    // flag to check the the chair is fully constructed from wheels/profile componenents - prevent load order errors
    private bool hasInitializedProfile = false;

    // note for me - this is the init
    private void Awake()
    {
        // set reference to the rb component
        chair_rb = GetComponent<Rigidbody>();
        // store children "wheels" in array
        wheels_array = GetComponentsInChildren<wheelscript>();
    }

    // note for me - this is the ready
    private void Start()
    {
        // grab the profile from the first available wheel on start as all wheels are the same
        if (wheels_array.Length > 0 && wheels_array[0].Profile != null)
        {
            // store this in the shared profile reference
            sharedProfile = wheels_array[0].Profile;
            // flag the chair has its wheels so can proceed
            hasInitializedProfile = true;
        }
    }

    // note for me - physics update loop
    private void FixedUpdate()
    {
        // create vector for horizontal movment
        // linear velocity is meters per second in world space axes  x - left or right, z - forward or backward
        // we remove the y here for up/down storing it as 0
        Vector3 horizontalVelocity = new Vector3(chair_rb.linearVelocity.x, 0f, chair_rb.linearVelocity.z);

        // wheels on ground counter
        int groundedWheelsCount = 0;

        // count how many wheels are touching the ground
        foreach (var wheel in wheels_array)
        {
            if (wheel.IsGrounded)
            {
                groundedWheelsCount++;
            }
        }

        // rolling resistance using the profile data
        // if at least one wheel is on the ground
        // the chair is moving at faster than the base line of stoppped which is 0.5f
        // and the chair has an initialised set of wheels (the flag to prevent initialisation error crash)
        if (groundedWheelsCount > 0 && horizontalVelocity.magnitude > 0.05f && hasInitializedProfile)
        {
            // use profile friction
            // apply force but reverse that force against the movement (creating a slowing affect calculated from friction)
            // normalise velocity to find direction of travel
            // multiply by the friction of the wheels
            // FORCE MODE ACCEL - add a continuous acceleration to the rigidbody, ignoring its mass
            // this allows testing of wheel function/feel without other elements
            // we could change this for chair mass as a value? weight of chair would affect speed maybe?
            chair_rb.AddForce(-horizontalVelocity.normalized * sharedProfile.rollingFriction, ForceMode.Acceleration);
        }

        // when the chair is barely moving set to stand still to stop creeping
        else if (horizontalVelocity.magnitude <= 0.05f)
        {
            chair_rb.linearVelocity = new Vector3(0f, chair_rb.linearVelocity.y, 0f);
        }

        // cap the max speed
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            // scale for the max speed
            Vector3 capped = horizontalVelocity.normalized * maxSpeed;
            // apply to the chair rigid body
            chair_rb.linearVelocity = new Vector3(capped.x, chair_rb.linearVelocity.y, capped.z);
        }
    }

    // function to shove the chair - taking a hit point of the push and a force
    public void PushChair(Vector3 force, Vector3 hitPoint)
    {
        chair_rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }
}