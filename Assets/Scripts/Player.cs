using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement, change 'linear dampening' for drag")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float maxMoveSpeed = 20f;
    
    [Header("Rotation")]
    [SerializeField] private float deadzone = 0.2f;
    [SerializeField] private float turnSpeed = 200f;
    
    [Header("Push")]
    [SerializeField] private float pushAmount = 6.5f;
    [SerializeField] private float pushCooldown = 1f;
    [SerializeField] private Color cooldownColor = Color.red;
    
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float currentRotation;
    private float nextPushTime;

    private SpriteRenderer sr;
    private Color normalColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        currentRotation = rb.rotation;
        normalColor = sr.color;
    }
    
    void Update()
    {
        bool onCooldown = Time.time < nextPushTime;
        sr.color = onCooldown ? cooldownColor : normalColor;
    }

    void FixedUpdate()
    {
        // normal "slow" movement
        rb.AddForce(moveInput * moveForce);
        
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxMoveSpeed);
        
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, currentRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > deadzone * deadzone)
        {
            float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            currentRotation = angle - 90f;
        }

    }
    
    public void Push(InputAction.CallbackContext context)
    {
        // Only fire once when the button is pressed, and only if the cooldown has passed
        if (!context.performed || Time.time < nextPushTime) return;

        nextPushTime = Time.time + pushCooldown;
        rb.AddForce(transform.up * pushAmount, ForceMode2D.Impulse);
    }
}
