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
            if (!_targetGroup) return;
            if (_playerRigidbody.isKinematic) return;
            var center = _targetGroup.transform.position;
            var position = _playerRigidbody.position;
            
            float hw = boxWidth * 0.5f, hl = boxLength * 0.5f;

            Vector3 clamped = new Vector3(
                Mathf.Clamp(position.x, center.x - hw, center.x + hw),
                position.y,
                Mathf.Clamp(position.z, center.z - hl, center.z + hl));

            if (clamped == position) return; // inside the box
            
            // stop velocity only on the axis that hit the edge, so the player slides along it
            var v = _playerRigidbody.linearVelocity;
            if (clamped.x != position.x) v.x = 0f;
            if (clamped.z != position.z) v.z = 0f;
            
            _playerRigidbody.position = clamped;
            _playerRigidbody.linearVelocity = v;
        }
    }
}