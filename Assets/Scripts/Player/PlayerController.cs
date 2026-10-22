using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float acceleration = 20f;
    [SerializeField] float maxSpeed = 8f;
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] float minFacingSpeed = 0.1f;
    [SerializeField] float driftAnglemin = 30f;
    [SerializeField] float driftAnglemax = 100f;
    [SerializeField] float normalSmoothing = 30f;
    [SerializeField] float maxNormalAngle = 30f;

    GameObject characterModel;
    Rigidbody rb;
    InputAction moveAction;
    ParticleSystem DrifterParticles;
    Vector3 facingDirection = Vector3.forward;   
    Vector3 inputDirection = Vector3.zero;
    Vector3 movingDirection = Vector3.zero;


    Vector3 smoothedUp = Vector3.up;
    bool grounded = true;

    void Start()
    {
        characterModel = transform.Find("Player Model").gameObject;

        DrifterParticles = characterModel.transform.Find("ParticleEmmiter").gameObject.GetComponent<ParticleSystem>();
        DrifterParticles.Stop();

        rb = GetComponent<Rigidbody>();
        moveAction = GetComponent<PlayerInput>().actions["Move"];
    }

    // Movement Logic
    void FixedUpdate()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        inputDirection = new Vector3(input.x, 0f, input.y);

        // thanks random online tutorial :D
        Vector3 flatFacing = Vector3.ProjectOnPlane(movingDirection, Vector3.up).normalized;
        if (flatFacing.sqrMagnitude > 0.0001f)
            inputDirection = Quaternion.LookRotation(flatFacing, Vector3.up) * inputDirection;

        if (grounded)
        {
            rb.AddForce(inputDirection * acceleration, ForceMode.Acceleration);
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > maxSpeed)
            {
                Vector3 capped = flatVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(capped.x, rb.linearVelocity.y, capped.z);
            }
        }
    }

void Update()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
            return;


        grounded = (hit.distance <= 1.1);

        float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
        if (slopeAngle > maxNormalAngle)
            hit.normal = Vector3.up;

        smoothedUp = Vector3.Slerp(smoothedUp, hit.normal, normalSmoothing * Time.deltaTime);

        movingDirection = Vector3.ProjectOnPlane(rb.linearVelocity, smoothedUp);
        facingDirection = Vector3.Slerp(facingDirection, inputDirection, turnSpeed * Time.deltaTime);


        float driftAngle = Vector3.Angle(movingDirection, facingDirection);
        if (driftAngle > driftAnglemin &&  driftAngle < driftAnglemax && grounded)
        {
            DrifterParticles.Play();
        }
        else
        {
            DrifterParticles.Stop();
        }

        Vector3 flatFacing = Vector3.ProjectOnPlane(facingDirection, smoothedUp);
        if (flatFacing.sqrMagnitude < 0.0001f)
            flatFacing = Vector3.ProjectOnPlane(characterModel.transform.forward, smoothedUp);

        characterModel.transform.rotation = Quaternion.LookRotation(flatFacing, smoothedUp);
    }
}