using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
public class PlayerController : MonoBehaviour
{
    [Header("Movement")] // headers for easier navigation
    [SerializeField] float acceleration = 10f;
    [SerializeField] float maxSpeed = 20f;
    [Tooltip("Drag, lower = slidier chair")]
    [SerializeField] float slideDrag = 0.5f;
    
    [Header("Rotation")]
    [Tooltip("Stick must past this before the chair turns")]
    [SerializeField] float deadzone = 0.2f; 
    [SerializeField] float turnSpeed = 6f;
    
    [Header("Drifting")] 
    [SerializeField] float driftAngleMin = 30f;
    [SerializeField] float driftAngleMax = 100f;
    [SerializeField] float driftSpeedMin = 3f;
    
    [Header("Ground check and tilt")]
    [Tooltip("Max distance to the floor that still counts as grounded")]
    [SerializeField] private float groundCheckDistance = 1.1f;
    [SerializeField] float maxNormalAngle = 30f;
    [SerializeField] float normalSmoothing = 30f;
    [SerializeField] LayerMask groundMask;
    
    [Header("Push")] 
    [SerializeField] private float pushAmount = 40f;
    [SerializeField] private float pushCooldown = 1f;
    [SerializeField] private Color cooldownColor = Color.red;

    // References set up in Awake()
    private GameObject _characterModel;
    private Rigidbody _rb;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _pushAction;
    private ParticleSystem _driftParticles;
    private Material _modelMaterial;
    private Color _normalColor;
    
    // State
    private Vector3 _facingDirection = Vector3.forward; // direction the chair visually faces
    private Vector3 _targetFacing = Vector3.forward;    // last direction the stick was pushed 
    private Vector3 _smoothedUp = Vector3.up;           // used to tilt the model
    private bool _grounded = true;
    private float _nextPushTime;
    
    private Vector3 _movingDirection = Vector3.zero;
    private Vector3 _flatVel = Vector3.zero;

    private bool PushOnCooldown => Time.time < _nextPushTime;


    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>(); 
    }

    private void Start()
    {
        _characterModel = transform.Find("Player Model").gameObject;

        _driftParticles = _characterModel.transform.Find("ParticleEmmiter").gameObject.GetComponent<ParticleSystem>();
        _driftParticles.Stop();
        
        _moveAction = _playerInput.actions["Move"];
        _pushAction = _playerInput.actions["Push"];

        // MeshRenderer on purpose: GetComponentInChildren<Renderer> could grab the particle renderer.
        // Use SkinnedMeshRenderer instead if the chair model is rigged.
        MeshRenderer meshRenderer = _characterModel.GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            _modelMaterial = meshRenderer.material;
            _normalColor = _modelMaterial.color;
        }

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Movement Logic
    private void FixedUpdate()
    {
        // // thanks random online tutorial :D
        // Vector3 flatFacing = Vector3.ProjectOnPlane(_movingDirection, Vector3.up).normalized;
        // if (flatFacing.sqrMagnitude > 0.0001f)
        //     _moveAmt = Quaternion.LookRotation(flatFacing, Vector3.up) * _moveAmt;
        
        if (!_grounded)
            return;

        // keep pushing the chair in the input direction
        _rb.AddForce(ReadMoveInput() * acceleration, ForceMode.Acceleration);

        Vector3 vel = _rb.linearVelocity;
        _flatVel = new Vector3(vel.x, 0f, vel.z);

        // slidey drag on the floor plane only (so gravity/falling is unaffected)
        _flatVel *= Mathf.Clamp01(1f - slideDrag * Time.fixedDeltaTime);

        // speed cap
        _flatVel = Vector3.ClampMagnitude(_flatVel, maxSpeed);

        _rb.linearVelocity = new Vector3(_flatVel.x, vel.y, _flatVel.z);
    }

    private void Update()
    {
        UpdateGroundState();
        
        // input check for _pushAction
        if (_pushAction.WasPressedThisFrame())
        {
            Push();
        }

        UpdateFacing();
        UpdateDriftParticles();
        UpdatePushTint();
    }

    private void Push()
    {
        if (!_grounded || PushOnCooldown)
            return;

        _nextPushTime = Time.time + pushCooldown;

        // Shove the way the chair is facing, along the ground.
        Vector3 pushDirection = Vector3.ProjectOnPlane(_facingDirection, _smoothedUp).normalized;
        _rb.AddForce(pushDirection * pushAmount, ForceMode.Impulse);
    }

    private Vector3 ReadMoveInput()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        return new Vector3(input.x, 0f, input.y);
    }
    
    private void UpdateGroundState()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 
                groundCheckDistance * 2f, groundMask, QueryTriggerInteraction.Ignore))

        _grounded = hit.distance <= groundCheckDistance;

        // Very steep slopes are treated as flat so the model doesn't tilt wildly.
        Vector3 normal = Vector3.Angle(hit.normal, Vector3.up) > maxNormalAngle ? Vector3.up : hit.normal;
        _smoothedUp = Vector3.Slerp(_smoothedUp, normal, normalSmoothing * Time.deltaTime);
    }
    
    private void UpdatePushTint()
    {
        if (_modelMaterial != null)
            _modelMaterial.color = PushOnCooldown ? cooldownColor : _normalColor;
    }

    // Facing & model

    private void UpdateFacing()
    {
        // Only turn towards the stick when it's past the deadzone, otherwise keep the last facing.
        Vector3 input = ReadMoveInput();
        if (input.sqrMagnitude > deadzone * deadzone)
            _targetFacing = input.normalized;

        _facingDirection = Vector3.Slerp(_facingDirection, _targetFacing, turnSpeed * Time.deltaTime);
        
        Vector3 forward = Vector3.ProjectOnPlane(_facingDirection, _smoothedUp);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.ProjectOnPlane(_characterModel.transform.forward, _smoothedUp);

        _characterModel.transform.rotation = Quaternion.LookRotation(forward, _smoothedUp);
    }
    // Drifting
    private void UpdateDriftParticles()
    {
        _movingDirection = Vector3.ProjectOnPlane(_rb.linearVelocity, _smoothedUp);

        float driftAngle = Vector3.Angle(_movingDirection, _facingDirection);
        if (driftAngle > driftAngleMin && driftAngle < driftAngleMax
                                       && _grounded
                                       && _flatVel.magnitude >= driftSpeedMin)
        {
            _driftParticles.Play();
        }
        else
        {
            _driftParticles.Stop();
        }
    }
}
}