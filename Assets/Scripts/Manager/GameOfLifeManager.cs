using UnityEngine;
using GameOfLife.Core;

namespace GameOfLife.Manager
{
    /// <summary>
    /// 콘웨이의 생명 게임 규칙을 적용하고 틱 시스템을 관리합니다.
    /// </summary>
    public class GameOfLifeManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 50;
        [SerializeField] private int gridHeight = 50;
        [SerializeField] private float cellSize = 1f;

        [Header("Tick Settings")]
        [SerializeField] private float tickRate = 1f; // 틱 속도 (1초 = 1틱/초)

        [Header("Stage Settings")]
        [SerializeField] private int currentStageIndex = 0;
        [SerializeField] private StageConfig[] stageConfigs = new StageConfig[5]
        {
            new StageConfig { ruleType = GameRuleType.ConwayLife, playerStartPosition = new Vector2Int(8, 8), kernelPosition = new Vector2Int(42, 22) },
            new StageConfig { ruleType = GameRuleType.HighLife, playerStartPosition = new Vector2Int(7, 10), kernelPosition = new Vector2Int(40, 20) },
            new StageConfig { ruleType = GameRuleType.Maze, playerStartPosition = new Vector2Int(7, 7), kernelPosition = new Vector2Int(43, 23) },
            new StageConfig { ruleType = GameRuleType.DayAndNight, playerStartPosition = new Vector2Int(15, 15), kernelPosition = new Vector2Int(38, 22) },
            new StageConfig { ruleType = GameRuleType.Seeds, playerStartPosition = new Vector2Int(25, 22), kernelPosition = new Vector2Int(25, 8) }
        };
        [SerializeField] private bool spawnInitialPattern = true;

        [Header("Auto Spawn Settings")]
        [SerializeField] private bool autoSpawnPatterns = false;
        [SerializeField] private float spawnInterval = 15f;

        private GridManager gridManager;
        private float tickTimer;
        private float spawnTimer;
        private int tickCount = 0;
        private KernelObject kernelObject; // 목표 지점 오브젝트

        public GridManager Grid => gridManager;
        public float TickRate => tickRate;
        public GameRuleType CurrentStage => stageConfigs[currentStageIndex].ruleType;
        public Vector2Int KernelPosition => stageConfigs[currentStageIndex].kernelPosition;
        public Vector2Int PlayerStartPosition => stageConfigs[currentStageIndex].playerStartPosition;
        public KernelObject Kernel => kernelObject;

        void Awake()
        {
            InitializeGrid();
            InitializeStageConfigs();
        }

        void Start()
        {
            if (spawnInitialPattern)
            {
                LoadStageByIndex(currentStageIndex);
            }
        }

        private void InitializeStageConfigs()
        {
            // stageConfigs가 null이거나 비어있으면 기본값으로 초기화
            if (stageConfigs == null || stageConfigs.Length != 5)
            {
                Debug.LogWarning("StageConfigs not properly initialized. Creating default configs.");
                stageConfigs = new StageConfig[5]
                {
                    new StageConfig { ruleType = GameRuleType.ConwayLife, playerStartPosition = new Vector2Int(8, 8), kernelPosition = new Vector2Int(42, 22) },
                    new StageConfig { ruleType = GameRuleType.HighLife, playerStartPosition = new Vector2Int(7, 10), kernelPosition = new Vector2Int(40, 20) },
                    new StageConfig { ruleType = GameRuleType.Maze, playerStartPosition = new Vector2Int(7, 7), kernelPosition = new Vector2Int(43, 23) },
                    new StageConfig { ruleType = GameRuleType.DayAndNight, playerStartPosition = new Vector2Int(15, 15), kernelPosition = new Vector2Int(38, 22) },
                    new StageConfig { ruleType = GameRuleType.Seeds, playerStartPosition = new Vector2Int(25, 22), kernelPosition = new Vector2Int(25, 8) }
                };
            }

            // currentStageIndex가 범위를 벗어나면 0으로 초기화
            if (currentStageIndex < 0 || currentStageIndex >= stageConfigs.Length)
            {
                currentStageIndex = 0;
            }
        }

        void Update()
        {
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickRate)
            {
                tickTimer = 0f;
                ProcessTick();
            }

            // 자동 패턴 스폰
            if (autoSpawnPatterns)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    spawnTimer = 0f;
                    SpawnRandomPattern();
                }
            }
        }

        private void InitializeGrid()
        {
            gridManager = new GridManager(gridWidth, gridHeight, cellSize);
        }

        /// <summary>
        /// 한 틱을 처리합니다. (콘웨이의 생명 게임 규칙 적용)
        /// </summary>
        private void ProcessTick()
        {
            // 1단계: 다음 상태 계산
            CalculateNextState();

            // 2단계: 다음 상태 적용
            ApplyNextState();

            tickCount++;
        }

        /// <summary>
        /// 현재 스테이지의 규칙에 따라 다음 상태를 계산합니다.
        /// </summary>
        private void CalculateNextState()
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Cell cell = gridManager.GetCell(x, y);

                    // Permanent(벽)와 Core(코어)는 생명 로직 적용 제외
                    if (cell.Type == CellType.Permanent || cell.Type == CellType.Core)
                    {
                        cell.WillBeAlive = cell.IsAlive;
                        continue;
                    }

                    int liveNeighbors = gridManager.CountLiveNeighbors(x, y);

                    // 스테이지별 규칙 적용
                    switch (stageConfigs[currentStageIndex].ruleType)
                    {
                        case GameRuleType.ConwayLife:
                            ApplyConwayRule(cell, liveNeighbors);
                            break;
                        case GameRuleType.HighLife:
                            ApplyHighLifeRule(cell, liveNeighbors);
                            break;
                        case GameRuleType.Maze:
                            ApplyMazeRule(cell, liveNeighbors);
                            break;
                        case GameRuleType.DayAndNight:
                            ApplyDayAndNightRule(cell, liveNeighbors);
                            break;
                        case GameRuleType.Seeds:
                            ApplySeedsRule(cell, liveNeighbors);
                            break;
                    }
                }
            }
        }

        // === 각 규칙 구현 ===

        private void ApplyConwayRule(Cell cell, int neighbors)
        {
            // B3/S23 - 기본 콘웨이
            if (cell.IsAlive)
            {
                cell.WillBeAlive = (neighbors == 2 || neighbors == 3);
            }
            else
            {
                cell.WillBeAlive = (neighbors == 3);
                if (cell.WillBeAlive) cell.Type = CellType.Normal;
            }
        }

        private void ApplyHighLifeRule(Cell cell, int neighbors)
        {
            // B36/S23 - Replicator 패턴 존재
            if (cell.IsAlive)
            {
                cell.WillBeAlive = (neighbors == 2 || neighbors == 3);
            }
            else
            {
                cell.WillBeAlive = (neighbors == 3 || neighbors == 6);
                if (cell.WillBeAlive) cell.Type = CellType.Normal;
            }
        }

        private void ApplyMazeRule(Cell cell, int neighbors)
        {
            // B3/S12345 - 미로 생성
            if (cell.IsAlive)
            {
                cell.WillBeAlive = (neighbors >= 1 && neighbors <= 5);
            }
            else
            {
                cell.WillBeAlive = (neighbors == 3);
                if (cell.WillBeAlive) cell.Type = CellType.Normal;
            }
        }

        private void ApplyDayAndNightRule(Cell cell, int neighbors)
        {
            // B3678/S34678 - 대칭 규칙
            if (cell.IsAlive)
            {
                cell.WillBeAlive = (neighbors == 3 || neighbors == 4 ||
                                   neighbors == 6 || neighbors == 7 || neighbors == 8);
            }
            else
            {
                cell.WillBeAlive = (neighbors == 3 || neighbors == 6 ||
                                   neighbors == 7 || neighbors == 8);
                if (cell.WillBeAlive) cell.Type = CellType.Normal;
            }
        }

        private void ApplySeedsRule(Cell cell, int neighbors)
        {
            // B2/S - 모든 세포가 1틱만 생존 (폭발형)
            if (cell.IsAlive)
            {
                cell.WillBeAlive = false; // 항상 죽음
            }
            else
            {
                cell.WillBeAlive = (neighbors == 2);
                if (cell.WillBeAlive) cell.Type = CellType.Normal;
            }
        }

        /// <summary>
        /// 계산된 다음 상태를 실제로 적용합니다.
        /// </summary>
        private void ApplyNextState()
        {
            foreach (var cell in gridManager.GetAllCells())
            {
                cell.ApplyNextState();
            }
        }

        // === 스테이지 로드 시스템 ===

        public void LoadStageByIndex(int stageIndex)
        {
            if (stageIndex < 0 || stageIndex >= stageConfigs.Length)
            {
                Debug.LogError($"Invalid stage index: {stageIndex}");
                return;
            }

            currentStageIndex = stageIndex;
            LoadStage(stageConfigs[stageIndex].ruleType);
        }

        public void LoadStage(GameRuleType stage)
        {
            // stageConfigs에서 해당 stage의 인덱스를 찾아서 currentStageIndex 설정
            for (int i = 0; i < stageConfigs.Length; i++)
            {
                if (stageConfigs[i].ruleType == stage)
                {
                    currentStageIndex = i;
                    break;
                }
            }

            ClearGrid();
            CreateKernel(); // 커널 생성 또는 위치 업데이트

            switch (stage)
            {
                case GameRuleType.ConwayLife:
                    LoadStage1_Conway();
                    break;
                case GameRuleType.HighLife:
                    LoadStage2_HighLife();
                    break;
                case GameRuleType.Maze:
                    LoadStage3_Maze();
                    break;
                case GameRuleType.DayAndNight:
                    LoadStage4_DayAndNight();
                    break;
                case GameRuleType.Seeds:
                    LoadStage5_Seeds();
                    break;
            }

            Debug.Log($"Loaded Stage: {stage}");
        }

        private void CreateKernel()
        {
            Vector2Int kernelGridPos = stageConfigs[currentStageIndex].kernelPosition;
            Vector3 kernelWorldPos = gridManager.GridToWorldPosition(kernelGridPos.x, kernelGridPos.y);

            if (kernelObject == null)
            {
                // 커널 오브젝트 생성
                GameObject kernelGO = new GameObject("Kernel");
                kernelGO.transform.parent = transform;
                kernelObject = kernelGO.AddComponent<KernelObject>();

                // SpriteRenderer 추가 (KernelObject가 자동으로 설정)
                kernelGO.AddComponent<SpriteRenderer>();
            }

            // 커널 위치 설정
            kernelObject.SetPosition(kernelWorldPos);
            Debug.Log($"Kernel created at grid position {kernelGridPos}, world position {kernelWorldPos}");
        }

        private void ClearGrid()
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    gridManager.SetCellAlive(x, y, false);
                }
            }
        }

        // === 스테이지별 레벨 디자인 ===

        private void LoadStage1_Conway()
        {
            // 스테이지 1: 튜토리얼 - 기본 콘웨이 규칙 (B3/S23)
            CreateStageBoundary();

            // 간단한 맵 구조 (Permanent 벽) - 벽 사이 간격을 넓게
            CreateHorizontalPlatform(8, 10, 10);
            CreateHorizontalPlatform(32, 18, 10);

            // 코어 클러스터 배치 (3개 - 튜토리얼) - 벽과의 거리 고려 (radius=4, 벽에서 최소 6유닛 이상 떨어짐)
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(13, 15), normalCellRadius = 4, normalCellCount = 15 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(25, 13), normalCellRadius = 4, normalCellCount = 15 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(38, 14), normalCellRadius = 4, normalCellCount = 15 });
        }

        private void LoadStage2_HighLife()
        {
            // 스테이지 2: HighLife - Replicator 패턴 (B36/S23)
            CreateStageBoundary();

            // 미로형 맵 구조 (Permanent 벽) - 간격을 넓게
            CreateHorizontalPlatform(10, 8, 8);
            CreateHorizontalPlatform(32, 18, 8);
            CreateVerticalWall(20, 12, 5);

            // 코어 클러스터 배치 (4개) - 벽과의 거리 고려 (radius=5, 벽에서 최소 7유닛 이상 떨어짐)
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(13, 14), normalCellRadius = 5, normalCellCount = 20 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(28, 13), normalCellRadius = 5, normalCellCount = 20 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(35, 13), normalCellRadius = 5, normalCellCount = 20 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(40, 20), normalCellRadius = 5, normalCellCount = 20 });
        }

        private void LoadStage3_Maze()
        {
            // 스테이지 3: Maze - 미로 생성 규칙 (B3/S12345)
            CreateStageBoundary();

            // 복잡한 미로 구조 (Permanent 벽) - 간격을 넓게
            CreateHorizontalPlatform(8, 8, 6);
            CreateHorizontalPlatform(24, 15, 6);
            CreateHorizontalPlatform(38, 20, 6);
            CreateVerticalWall(16, 11, 5);
            CreateVerticalWall(32, 11, 5);

            // 코어 클러스터 배치 (5개) - 벽과의 거리 고려 (radius=5, 벽에서 최소 7유닛 이상 떨어짐)
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(11, 14), normalCellRadius = 5, normalCellCount = 22 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(22, 11), normalCellRadius = 5, normalCellCount = 22 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(28, 18), normalCellRadius = 5, normalCellCount = 22 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(38, 13), normalCellRadius = 5, normalCellCount = 22 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(42, 16), normalCellRadius = 5, normalCellCount = 22 });
        }

        private void LoadStage4_DayAndNight()
        {
            // 스테이지 4: Day & Night - 매우 활발한 규칙 (B3678/S34678)
            CreateStageBoundary();

            // 복잡한 구조물 (Permanent 벽) - 간격을 넓게
            CreateBox(10, 8, 3, 3);
            CreateBox(24, 17, 4, 3);
            CreateBox(36, 8, 3, 4);
            CreateVerticalWall(18, 12, 5);

            // 코어 클러스터 배치 (6개 - 높은 난이도) - 벽과의 거리 고려 (radius=6, 벽에서 최소 8유닛 이상 떨어짐)
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(15, 14), normalCellRadius = 6, normalCellCount = 25 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(30, 10), normalCellRadius = 6, normalCellCount = 25 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(20, 20), normalCellRadius = 6, normalCellCount = 25 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(33, 14), normalCellRadius = 6, normalCellCount = 25 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(42, 12), normalCellRadius = 6, normalCellCount = 25 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(38, 20), normalCellRadius = 6, normalCellCount = 25 });
        }

        private void LoadStage5_Seeds()
        {
            // 스테이지 5: Seeds - 폭발형 (최고 난이도) (B2/S)
            CreateStageBoundary();

            // 매우 복잡한 맵 구조 (Permanent 벽) - 간격을 넓게
            CreateBox(8, 7, 3, 3);
            CreateBox(18, 15, 3, 3);
            CreateBox(28, 7, 3, 3);
            CreateBox(38, 18, 3, 3);
            CreateVerticalWall(14, 11, 4);
            CreateVerticalWall(34, 11, 4);
            CreateHorizontalPlatform(10, 21, 8);
            CreateHorizontalPlatform(32, 21, 8);

            // 코어 클러스터 배치 (7개 - 최고 난이도) - 벽과의 거리 고려 (radius=6, 벽에서 최소 8유닛 이상 떨어짐)
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(22, 10), normalCellRadius = 6, normalCellCount = 28 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(12, 18), normalCellRadius = 6, normalCellCount = 28 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(26, 19), normalCellRadius = 6, normalCellCount = 28 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(40, 11), normalCellRadius = 6, normalCellCount = 28 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(32, 14), normalCellRadius = 6, normalCellCount = 28 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(42, 20), normalCellRadius = 6, normalCellCount = 28 });
            CreateCluster(new ClusterConfig { corePosition = new Vector2Int(20, 15), normalCellRadius = 6, normalCellCount = 28 });
        }

        private void CreateStageBoundary()
        {
            // 외곽 경계 생성
            CreateHorizontalPlatform(5, 5, 40);
            CreateHorizontalPlatform(5, 24, 40);
            CreateVerticalWall(5, 5, 20);
            CreateVerticalWall(44, 5, 20);
        }

        private void SpawnReplicatorPattern(int startX, int startY)
        {
            // HighLife 전용 Replicator 씨앗
            gridManager.SetCellAlive(startX, startY, true);
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX + 2, startY, true);
            gridManager.SetCellAlive(startX, startY + 1, true);
            gridManager.SetCellAlive(startX + 1, startY + 2, true);
        }

        // === 초기 패턴 생성 메서드 ===

        private void SpawnGliderPattern(int startX, int startY)
        {
            // Glider 패턴: 우하향으로 이동하는 패턴
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX + 2, startY + 1, true);
            gridManager.SetCellAlive(startX, startY + 2, true);
            gridManager.SetCellAlive(startX + 1, startY + 2, true);
            gridManager.SetCellAlive(startX + 2, startY + 2, true);
        }

        private void SpawnBlinkerPattern(int startX, int startY)
        {
            // Blinker 패턴: 수직/수평으로 진동하는 패턴
            gridManager.SetCellAlive(startX, startY, true);
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX + 2, startY, true);
        }

        private void SpawnBlockPattern(int startX, int startY)
        {
            // Block 패턴: 정적인 패턴 (변하지 않음)
            gridManager.SetCellAlive(startX, startY, true);
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX, startY + 1, true);
            gridManager.SetCellAlive(startX + 1, startY + 1, true);
        }

        private void SpawnMazePattern()
        {
            // 플랫포머 스타일 미로 생성
            int centerX = gridWidth / 2;
            int centerY = gridHeight / 2;

            // 바닥 (하단)
            CreateHorizontalPlatform(5, 5, 40);

            // 왼쪽 벽
            CreateVerticalWall(5, 5, 20);

            // 오른쪽 벽
            CreateVerticalWall(44, 5, 20);

            // 상단 벽
            CreateHorizontalPlatform(5, 24, 40);

            // 플랫폼들
            CreateHorizontalPlatform(10, 10, 8);
            CreateHorizontalPlatform(30, 10, 8);

            CreateHorizontalPlatform(15, 15, 10);
            CreateHorizontalPlatform(28, 15, 10);

            CreateHorizontalPlatform(8, 20, 6);
            CreateHorizontalPlatform(20, 20, 10);
            CreateHorizontalPlatform(35, 20, 6);

            // 중앙 장애물
            CreateBox(22, 8, 6, 3);

            // 좌우 계단식 구조
            CreateStairs(8, 6, 5, true);  // 왼쪽 올라가는 계단
            CreateStairs(38, 6, 5, false); // 오른쪽 내려가는 계단

            // 증식 패턴들 (게임을 활성화)
            SpawnGliderGunPattern(10, 12); // 왼쪽에 Glider Gun
            SpawnGliderGunPattern(35, 18); // 오른쪽에 Glider Gun
            SpawnRPentominoPattern(centerX, centerY + 5); // 중앙에 R-pentomino (카오스)
            SpawnPulsarPattern(15, 8); // Pulsar 진동 패턴
            SpawnAcornPattern(30, 22); // Acorn 증식 패턴
        }

        private void CreateHorizontalPlatform(int startX, int y, int length, CellType type = CellType.Permanent)
        {
            for (int x = startX; x < startX + length && x < gridWidth; x++)
            {
                gridManager.SetCellAlive(x, y, true, type);
            }
        }

        private void CreateVerticalWall(int x, int startY, int height, CellType type = CellType.Permanent)
        {
            for (int y = startY; y < startY + height && y < gridHeight; y++)
            {
                gridManager.SetCellAlive(x, y, true, type);
            }
        }

        private void CreateBox(int startX, int startY, int width, int height, CellType type = CellType.Permanent)
        {
            for (int x = startX; x < startX + width && x < gridWidth; x++)
            {
                for (int y = startY; y < startY + height && y < gridHeight; y++)
                {
                    gridManager.SetCellAlive(x, y, true, type);
                }
            }
        }

        private void CreateStairs(int startX, int startY, int steps, bool ascending, CellType type = CellType.Permanent)
        {
            for (int i = 0; i < steps; i++)
            {
                int x = startX + i;
                int y = ascending ? startY + i : startY + (steps - i - 1);

                if (x < gridWidth && y < gridHeight)
                {
                    gridManager.SetCellAlive(x, y, true, type);
                }
            }
        }

        // === 증식/활성 패턴들 ===

        private void SpawnGliderGunPattern(int startX, int startY)
        {
            // Gosper's Glider Gun - 무한히 Glider를 생성하는 패턴
            // 왼쪽 블록
            gridManager.SetCellAlive(startX, startY, true);
            gridManager.SetCellAlive(startX, startY + 1, true);
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX + 1, startY + 1, true);

            // 왼쪽 생성기
            gridManager.SetCellAlive(startX + 10, startY, true);
            gridManager.SetCellAlive(startX + 10, startY + 1, true);
            gridManager.SetCellAlive(startX + 10, startY + 2, true);
            gridManager.SetCellAlive(startX + 11, startY - 1, true);
            gridManager.SetCellAlive(startX + 11, startY + 3, true);
            gridManager.SetCellAlive(startX + 12, startY - 2, true);
            gridManager.SetCellAlive(startX + 13, startY - 2, true);
            gridManager.SetCellAlive(startX + 12, startY + 4, true);
            gridManager.SetCellAlive(startX + 13, startY + 4, true);
            gridManager.SetCellAlive(startX + 14, startY + 1, true);
            gridManager.SetCellAlive(startX + 15, startY - 1, true);
            gridManager.SetCellAlive(startX + 15, startY + 3, true);
            gridManager.SetCellAlive(startX + 16, startY, true);
            gridManager.SetCellAlive(startX + 16, startY + 1, true);
            gridManager.SetCellAlive(startX + 16, startY + 2, true);
            gridManager.SetCellAlive(startX + 17, startY + 1, true);

            // 오른쪽 생성기
            gridManager.SetCellAlive(startX + 20, startY - 2, true);
            gridManager.SetCellAlive(startX + 20, startY - 3, true);
            gridManager.SetCellAlive(startX + 20, startY - 4, true);
            gridManager.SetCellAlive(startX + 21, startY - 2, true);
            gridManager.SetCellAlive(startX + 21, startY - 3, true);
            gridManager.SetCellAlive(startX + 21, startY - 4, true);
            gridManager.SetCellAlive(startX + 22, startY - 1, true);
            gridManager.SetCellAlive(startX + 22, startY - 5, true);
            gridManager.SetCellAlive(startX + 24, startY - 1, true);
            gridManager.SetCellAlive(startX + 24, startY, true);
            gridManager.SetCellAlive(startX + 24, startY - 5, true);
            gridManager.SetCellAlive(startX + 24, startY - 6, true);

            // 오른쪽 블록
            gridManager.SetCellAlive(startX + 34, startY - 3, true);
            gridManager.SetCellAlive(startX + 34, startY - 4, true);
            gridManager.SetCellAlive(startX + 35, startY - 3, true);
            gridManager.SetCellAlive(startX + 35, startY - 4, true);
        }

        private void SpawnRPentominoPattern(int startX, int startY)
        {
            // R-pentomino - 1103세대 동안 활성화되는 카오스 패턴
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX + 2, startY, true);
            gridManager.SetCellAlive(startX, startY + 1, true);
            gridManager.SetCellAlive(startX + 1, startY + 1, true);
            gridManager.SetCellAlive(startX + 1, startY + 2, true);
        }

        private void SpawnPulsarPattern(int startX, int startY)
        {
            // Pulsar - 주기 3의 진동 패턴
            int[][] pattern = new int[][]
            {
                new int[] {2, 0}, new int[] {3, 0}, new int[] {4, 0}, new int[] {8, 0}, new int[] {9, 0}, new int[] {10, 0},
                new int[] {0, 2}, new int[] {5, 2}, new int[] {7, 2}, new int[] {12, 2},
                new int[] {0, 3}, new int[] {5, 3}, new int[] {7, 3}, new int[] {12, 3},
                new int[] {0, 4}, new int[] {5, 4}, new int[] {7, 4}, new int[] {12, 4},
                new int[] {2, 5}, new int[] {3, 5}, new int[] {4, 5}, new int[] {8, 5}, new int[] {9, 5}, new int[] {10, 5},
                new int[] {2, 7}, new int[] {3, 7}, new int[] {4, 7}, new int[] {8, 7}, new int[] {9, 7}, new int[] {10, 7},
                new int[] {0, 8}, new int[] {5, 8}, new int[] {7, 8}, new int[] {12, 8},
                new int[] {0, 9}, new int[] {5, 9}, new int[] {7, 9}, new int[] {12, 9},
                new int[] {0, 10}, new int[] {5, 10}, new int[] {7, 10}, new int[] {12, 10},
                new int[] {2, 12}, new int[] {3, 12}, new int[] {4, 12}, new int[] {8, 12}, new int[] {9, 12}, new int[] {10, 12}
            };

            foreach (var pos in pattern)
            {
                gridManager.SetCellAlive(startX + pos[0], startY + pos[1], true);
            }
        }

        private void SpawnAcornPattern(int startX, int startY)
        {
            // Acorn - 5206세대 동안 활성화되며 증식하는 패턴
            gridManager.SetCellAlive(startX + 1, startY, true);
            gridManager.SetCellAlive(startX + 3, startY + 1, true);
            gridManager.SetCellAlive(startX, startY + 2, true);
            gridManager.SetCellAlive(startX + 1, startY + 2, true);
            gridManager.SetCellAlive(startX + 4, startY + 2, true);
            gridManager.SetCellAlive(startX + 5, startY + 2, true);
            gridManager.SetCellAlive(startX + 6, startY + 2, true);
        }

        private void SpawnRandomPattern()
        {
            // 미로 내부의 랜덤한 위치에 패턴 스폰
            int x = Random.Range(8, gridWidth - 12);
            int y = Random.Range(8, gridHeight - 8);

            // 랜덤한 패턴 선택
            int patternType = Random.Range(0, 4);

            switch (patternType)
            {
                case 0:
                    SpawnGliderPattern(x, y);
                    Debug.Log($"Spawned Glider at ({x}, {y})");
                    break;
                case 1:
                    SpawnRPentominoPattern(x, y);
                    Debug.Log($"Spawned R-pentomino at ({x}, {y})");
                    break;
                case 2:
                    SpawnAcornPattern(x, y);
                    Debug.Log($"Spawned Acorn at ({x}, {y})");
                    break;
                case 3:
                    SpawnPulsarPattern(x, y);
                    Debug.Log($"Spawned Pulsar at ({x}, {y})");
                    break;
            }
        }

        // === 플레이어 조작 메서드 ===

        /// <summary>
        /// 특정 위치의 세포를 삭제합니다 (좌클릭 공격)
        /// </summary>
        public void DeleteCell(int x, int y)
        {
            if (gridManager.IsInBounds(x, y))
            {
                gridManager.SetCellAlive(x, y, false);
            }
        }

        // === 클러스터 시스템 ===

        /// <summary>
        /// 클러스터를 생성합니다 (코어 + 주변 일반 셀)
        /// </summary>
        private void CreateCluster(ClusterConfig config)
        {
            if (config == null) return;

            // 코어 생성
            Vector2Int corePos = config.corePosition;
            gridManager.SetCellAlive(corePos.x, corePos.y, true, CellType.Core);

            // 코어 주변에 랜덤하게 일반 셀 배치
            int cellsPlaced = 0;
            int attempts = 0;
            int maxAttempts = config.normalCellCount * 3; // 무한루프 방지

            while (cellsPlaced < config.normalCellCount && attempts < maxAttempts)
            {
                attempts++;

                // 코어 주변 반경 내 랜덤 위치
                int offsetX = Random.Range(-config.normalCellRadius, config.normalCellRadius + 1);
                int offsetY = Random.Range(-config.normalCellRadius, config.normalCellRadius + 1);

                int x = corePos.x + offsetX;
                int y = corePos.y + offsetY;

                // 그리드 범위 체크
                if (!gridManager.IsInBounds(x, y)) continue;

                // 이미 셀이 있으면 스킵
                Cell cell = gridManager.GetCell(x, y);
                if (cell.IsAlive) continue;

                // 코어 위치는 스킵
                if (x == corePos.x && y == corePos.y) continue;

                // 일반 셀 배치
                gridManager.SetCellAlive(x, y, true, CellType.Normal);
                cellsPlaced++;
            }

            Debug.Log($"Created cluster at {corePos} with {cellsPlaced} normal cells");
        }

        /// <summary>
        /// 코어 파괴 시 코어 자체와 주변 일반 셀 제거
        /// </summary>
        public void DestroyCoreAndSurroundingCells(int coreX, int coreY, int radius = 10)
        {
            // 먼저 코어 자체를 파괴
            gridManager.SetCellAlive(coreX, coreY, false);

            // 반경 내 모든 Normal 셀 제거
            for (int x = coreX - radius; x <= coreX + radius; x++)
            {
                for (int y = coreY - radius; y <= coreY + radius; y++)
                {
                    if (!gridManager.IsInBounds(x, y)) continue;

                    // 거리 계산 (원형 범위)
                    float distance = Mathf.Sqrt((x - coreX) * (x - coreX) + (y - coreY) * (y - coreY));
                    if (distance > radius) continue;

                    Cell cell = gridManager.GetCell(x, y);
                    if (cell.Type == CellType.Normal)
                    {
                        gridManager.SetCellAlive(x, y, false);
                    }
                }
            }

            Debug.Log($"Core destroyed at ({coreX}, {coreY}), cleared {radius} radius");
        }
    }
}
