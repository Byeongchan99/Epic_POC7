using UnityEngine;

namespace GameOfLife.Camera
{
    /// <summary>
    /// 카메라가 플레이어를 부드럽게 따라다니도록 합니다.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private bool smoothFollow = true;

        private void Start()
        {
            // target이 설정되지 않았으면 플레이어를 찾음
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    Debug.Log("CameraController: Found player target");
                }
                else
                {
                    Debug.LogWarning("CameraController: Player not found! Make sure Player has 'Player' tag.");
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;

            if (smoothFollow)
            {
                // 부드러운 이동
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
                transform.position = smoothedPosition;
            }
            else
            {
                // 즉시 이동
                transform.position = desiredPosition;
            }
        }

        /// <summary>
        /// 카메라 타겟을 변경합니다.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// 카메라 오프셋을 변경합니다.
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }
    }
}
