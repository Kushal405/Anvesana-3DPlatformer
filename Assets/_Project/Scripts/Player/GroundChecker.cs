using UnityEngine;


    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] float groundDistance = 0.08f;
        [SerializeField] LayerMask groundLayers;

        public bool IsGrounded { get; private set; }

        void Update()
        {
            IsGrounded = Physics.SphereCast(
                transform.position,
                0.3f,
                Vector3.down,
                out _,
                groundDistance,
                groundLayers
            );
        }
    }
