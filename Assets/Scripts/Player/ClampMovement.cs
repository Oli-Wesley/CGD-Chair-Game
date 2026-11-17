using UnityEngine;
using Unity.Cinemachine;

namespace Player
{
    public class PlayerScreenClamper : MonoBehaviour
    {
        [Header("BOUNDARY SETTINGS")]
        [SerializeField] private float boxWidth = 55f;
        [SerializeField] private float boxLength = 35f;

        // variables to store rigid body and target group (for cinemachine)
        private CinemachineTargetGroup _targetGroup;
        private Rigidbody _playerRigidbody;

        // INIT?
        private void Start()
        {
            // on init get the components from player
            _playerRigidbody = GetComponent<Rigidbody>();
            _targetGroup = FindFirstObjectByType<CinemachineTargetGroup>();
        }

        // UPDATE LOOP
        private void FixedUpdate()
        {
            if (_targetGroup == null) return;

            // calculate Target Group (group of players) center (mean centroid)0
            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var target in _targetGroup.Targets)
            {
                if (target.Object != null)
                {
                    center += target.Object.position;
                    count++;
                }
            }
            if (count == 0) return;
            center /= count;

            // get x/z offset from center (ignore y)
            Vector3 offset = transform.position - center;
            float halfWidth = boxWidth / 2f;
            float halfLength = boxLength / 2f;

            // flag if out of the x/z extents of our box
            bool outOfX = Mathf.Abs(offset.x) > halfWidth;
            bool outOfZ = Mathf.Abs(offset.z) > halfLength;

            // clamp if outside extents
            if (outOfX || outOfZ)
            {
                // CLAMP forces player transform back into the box
                Vector3 clamped = center + new Vector3(
                    Mathf.Clamp(offset.x, -halfWidth, halfWidth),
                    0f,
                    Mathf.Clamp(offset.z, -halfLength, halfLength)
                );
                // preserve the height
                clamped.y = transform.position.y;


                // FIX FOR MOVE/SLIDE
                // REPLACES VELOCITY CHANGE IN ORIGINAL
                // zero the velocity on the specific out of extents axis so the other
                // movement is maintained causing a SLIDE

                // if rigid body is found - sense check to prevent error
                if (_playerRigidbody != null)
                {
                    // reposition player to clamped vector
                    _playerRigidbody.MovePosition(clamped);

                    // temporary variable to hold current velolicty
                    Vector3 currentVelocity = _playerRigidbody.linearVelocity;
                    // 0 out the movement on the axis if out of extents
                    if (outOfX) currentVelocity.x = 0f;
                    if (outOfZ) currentVelocity.z = 0f;
                    // overwrite the velocity preserving the other axis movements
                    _playerRigidbody.linearVelocity = currentVelocity;
                }
                // non physics object catch - ON WRONG OBJECT?
                else
                {
                    transform.position = clamped;
                }
            }
        }
    }
}