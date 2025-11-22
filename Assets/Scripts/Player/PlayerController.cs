using UnityEngine;
using GameOfLife.Manager;
using GameOfLife.Core;

namespace GameOfLife.Player
{
    /// <summary>
    /// 플레이어 이동, 충돌 감지, 입력 처리를 담당합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameOfLifeManager gameManager;
        [SerializeField] private Camera mainCamera;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Visual Settings")]
        [SerializeField] private Color playerColor = Color.red;

        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invincibilityDuration = 1f;

        private SpriteRenderer spriteRenderer;
        private int currentHealth;
        private float invincibilityTimer = 0f;
        private Vector2Int currentGridPos;

        public int CurrentHealth => currentHealth;
        public bool IsInvincible => invincibilityTimer > 0f;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = playerColor;
            currentHealth = maxHealth;
        }

        void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameOfLifeManager>();
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // 플레이어를 그리드 중앙에 배치
            transform.position = Vector3.zero;
            UpdateGridPosition();
        }

        void Update()
        {
            HandleMovement();
            HandleMouseInput();
            UpdateInvincibility();
            CheckCollisionWithEnemyCells();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 movement = new Vector3(horizontal, vertical, 0).normalized;
            transform.position += movement * moveSpeed * Time.deltaTime;

            UpdateGridPosition();
        }

        private void UpdateGridPosition()
        {
            if (gameManager != null && gameManager.Grid != null)
            {
                currentGridPos = gameManager.Grid.WorldToGridPosition(transform.position);
            }
        }

        private void HandleMouseInput()
        {
            if (gameManager == null || gameManager.Grid == null) return;

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridPos = gameManager.Grid.WorldToGridPosition(mouseWorldPos);

            // 좌클릭: 세포 삭제
            if (Input.GetMouseButton(0))
            {
                gameManager.DeleteCell(gridPos.x, gridPos.y);
            }

            // 우클릭: 세포 생성
            if (Input.GetMouseButtonDown(1))
            {
                gameManager.PlaceCell(gridPos.x, gridPos.y);
            }
        }

        private void UpdateInvincibility()
        {
            if (invincibilityTimer > 0f)
            {
                invincibilityTimer -= Time.deltaTime;

                // 무적 상태 시각 효과 (깜빡임)
                float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                Color color = playerColor;
                color.a = alpha;
                spriteRenderer.color = color;
            }
            else
            {
                spriteRenderer.color = playerColor;
            }
        }

        private void CheckCollisionWithEnemyCells()
        {
            if (IsInvincible || gameManager == null) return;

            Cell playerCell = gameManager.Grid.GetCell(currentGridPos.x, currentGridPos.y);

            if (playerCell != null && playerCell.IsAlive && playerCell.Type == CellType.Enemy)
            {
                TakeDamage();
            }
        }

        private void TakeDamage()
        {
            currentHealth--;
            invincibilityTimer = invincibilityDuration;

            Debug.Log($"Player damaged! Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            Debug.Log("Game Over!");
            // TODO: 게임 오버 처리
            Time.timeScale = 0f; // 게임 일시 정지
        }

        void OnDrawGizmos()
        {
            // 플레이어의 현재 그리드 위치 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}
