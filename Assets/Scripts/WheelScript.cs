using UnityEngine;

public class wheelscript : MonoBehaviour
{
    // wheel profile (ie standard/rubber etc)
    [SerializeField] private WheelProfile wheelProfile;
    // ground reference
    [SerializeField] private LayerMask groundLayer;
    // distance to floor
    [SerializeField] private float raycastLength = 0.2f;

    // is the wheel touching the ground
    private bool isGrounded;

    // getters
    public WheelProfile Profile => wheelProfile;
    public bool IsGrounded => isGrounded;

    private void Update()
    {
        // confirm that the wheel is grounded on update - if true its on the ground if false its floating
        isGrounded = Physics.Raycast(transform.position, Vector3.down, raycastLength, groundLayer);
    }

    // swap wheel function
    public void EquipWheel(WheelProfile newProfile)
    {
        wheelProfile = newProfile;
    }
}