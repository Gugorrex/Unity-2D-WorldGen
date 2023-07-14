using UnityEngine;

namespace _2D_WorldGen.Scripts.Testing
{
    public class DemoMoveController : MonoBehaviour
    {
        public float moveSpeed = 150;
        public float smoothTime = 0.5f;
        public float scrollSpeed = 5;

        private Camera _cam;
        private Vector3 _velocity = Vector3.zero;

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            var current = transform.position;
            var target = new Vector3(current.x + Input.GetAxis("Horizontal") * moveSpeed, 
                current.y + Input.GetAxis("Vertical") * moveSpeed,
                current.z);
            transform.position = Vector3.SmoothDamp(current, target, ref _velocity, smoothTime);
            _cam.orthographicSize -= Input.mouseScrollDelta.y * scrollSpeed;
        }
    }
}