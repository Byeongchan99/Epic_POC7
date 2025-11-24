using UnityEngine;
using GameOfLife.Manager;
using GameOfLife.Core;
using GameOfLife.UI;

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
        [SerializeField] private UnityEngine.Camera mainCamera;

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
                mainCamera = UnityEngine.Camera.main;
            }

            // 플레이어를 스테이지 시작 위치에 배치
            if (gameManager != null && gameManager.Grid != null)
            {
                Vector2Int startGridPos = gameManager.PlayerStartPosition;
                transform.position = gameManager.Grid.GridToWorldPosition(startGridPos.x, startGridPos.y);
            }
            UpdateGridPosition();
        }

        void Update()
        {
            HandleMovement();
            HandleMouseInput();
            HandleStageSwitch();
            UpdateInvincibility();
            CheckCollisionWithEnemyCells();
        }

        private void HandleStageSwitch()
        {
            // 1~5번 키로 스테이지 전환
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                LoadStage(GameRuleType.ConwayLife);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                LoadStage(GameRuleType.HighLife);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                LoadStage(GameRuleType.Maze);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                LoadStage(GameRuleType.DayAndNight);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                LoadStage(GameRuleType.Seeds);
            }
            // R키로 현재 스테이지 재시작
            else if (Input.GetKeyDown(KeyCode.R))
            {
                LoadStage(gameManager.CurrentStage);
            }
        }

        /// <summary>
        /// 플레이어 상태를 초기화합니다 (위치, 체력 등)
        /// </summary>
        public void ResetPlayerState()
        {
            if (gameManager == null || gameManager.Grid == null)
            {
                Debug.LogWarning("Cannot reset player state: GameManager or Grid is null");
                return;
            }

            // 체력 복구
            currentHealth = maxHealth;
            invincibilityTimer = 0f;

            // 플레이어 위치를 스테이지 시작 위치로 설정
            Vector2Int startGridPos = gameManager.PlayerStartPosition;
            transform.position = gameManager.Grid.GridToWorldPosition(startGridPos.x, startGridPos.y);

            Debug.Log($"Player state reset. Position: {startGridPos}, Health: {currentHealth}/{maxHealth}");
        }

        private void LoadStage(GameRuleType stage)
        {
            if (gameManager == null) return;

            // 타임스케일 정상화
            Time.timeScale = 1f;

            // 스테이지 로드
            gameManager.LoadStage(stage);

            // 플레이어 상태 초기화
            ResetPlayerState();

            Debug.Log($"Switched to Stage: {stage}");
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
            bullet.name = "Projectile"; // Instantiate된 오브젝트 이름 설정
            bullet.SetActive(true); // 활성화
            bullet.hideFlags = HideFlags.None; // Hierarchy에 표시

            Projectile proj = bullet.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(direction, gameManager);
            }
        }

        private void CreateDefaultProjectilePrefab()
        {
            // 기본 총알 프리팹 생성 (템플릿)
            projectilePrefab = new GameObject("ProjectilePrefab");

            SpriteRenderer sr = projectilePrefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = Color.yellow;

            projectilePrefab.AddComponent<Projectile>();
            projectilePrefab.transform.localScale = Vector3.one * 0.2f;

            // 템플릿 프리팹은 Hierarchy에서 숨기고 비활성화
            projectilePrefab.hideFlags = HideFlags.HideInHierarchy;
            projectilePrefab.SetActive(false);

            Debug.Log("Default projectile prefab created");
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
            if (gameManager == null || gameManager.Grid == null) return;

            // 커널 오브젝트와의 거리 체크
            if (gameManager.Kernel != null)
            {
                float distanceToKernel = Vector3.Distance(transform.position, gameManager.Kernel.transform.position);
                if (distanceToKernel < 0.5f) // 0.5 유닛 이내면 도달
                {
                    StageClear();
                    return;
                }
            }

            // 적 세포와 충돌 시 데미지 (개선된 판정)
            // 플레이어 주변 3x3 그리드 영역의 셀들을 체크
            if (!IsInvincible)
            {
                for (int offsetX = -1; offsetX <= 1; offsetX++)
                {
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        int checkX = currentGridPos.x + offsetX;
                        int checkY = currentGridPos.y + offsetY;

                        // 그리드 범위 체크
                        if (!gameManager.Grid.IsInBounds(checkX, checkY))
                            continue;

                        Cell cell = gameManager.Grid.GetCell(checkX, checkY);
                        if (cell != null && cell.IsAlive && cell.Type == CellType.Normal)
                        {
                            // 실제 거리 기반 충돌 판정
                            Vector3 cellWorldPos = gameManager.Grid.GridToWorldPosition(checkX, checkY);
                            float distance = Vector3.Distance(transform.position, cellWorldPos);

                            // 0.4 유닛 이내면 충돌로 판정 (셀 크기의 절반보다 약간 작게)
                            if (distance < 0.4f)
                            {
                                TakeDamage();
                                return; // 한 번만 데미지
                            }
                        }
                    }
                }
            }
        }

        private void StageClear()
        {
            Debug.Log("Stage Clear!");

            // 메인 메뉴 표시
            MainMenuUI mainMenu = FindFirstObjectByType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ShowMenu();
            }
            else
            {
                Debug.LogWarning("MainMenuUI not found!");
                Time.timeScale = 0f; // 일시 정지
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

            // 메인 메뉴 표시
            MainMenuUI mainMenu = FindFirstObjectByType<MainMenuUI>();
            if (mainMenu != null)
            {
                mainMenu.ShowMenu();
            }
            else
            {
                Debug.LogWarning("MainMenuUI not found!");
                Time.timeScale = 0f; // 게임 일시 정지
            }
        }

        void OnDrawGizmos()
        {
            // 플레이어의 현재 그리드 위치 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}
