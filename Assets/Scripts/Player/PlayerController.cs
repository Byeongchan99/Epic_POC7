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

        [Header("Shooting Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 0.2f; // 발사 간격 (초)
        [SerializeField] private Transform firePoint; // 총알 발사 위치 (없으면 플레이어 위치)

        private SpriteRenderer spriteRenderer;
        private float lastFireTime = 0f;
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
            mouseWorldPos.z = 0;

            // 좌클릭: 총 발사
            if (Input.GetMouseButton(0))
            {
                if (Time.time - lastFireTime >= fireRate)
                {
                    ShootProjectile(mouseWorldPos);
                    lastFireTime = Time.time;
                }
            }

            // 우클릭: 세포 생성
            if (Input.GetMouseButtonDown(1))
            {
                Vector2Int gridPos = gameManager.Grid.WorldToGridPosition(mouseWorldPos);
                gameManager.PlaceCell(gridPos.x, gridPos.y);
            }
        }

        private void ShootProjectile(Vector3 targetWorldPos)
        {
            if (projectilePrefab == null)
            {
                CreateDefaultProjectilePrefab();
            }

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector3 direction = (targetWorldPos - spawnPos).normalized;

            GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Projectile proj = bullet.GetComponent<Projectile>();

            if (proj != null)
            {
                proj.Initialize(direction, gameManager);
            }
        }

        private void CreateDefaultProjectilePrefab()
        {
            // 기본 총알 프리팹 생성
            projectilePrefab = new GameObject("Projectile");

            SpriteRenderer sr = projectilePrefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = Color.yellow;

            projectilePrefab.AddComponent<Projectile>();
            projectilePrefab.transform.localScale = Vector3.one * 0.2f;
        }

        private Sprite CreateCircleSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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
