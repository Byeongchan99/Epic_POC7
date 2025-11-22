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
        [SerializeField] private GameRuleType currentStage = GameRuleType.ConwayLife;
        [SerializeField] private bool spawnInitialPattern = true;

        [Header("Auto Spawn Settings")]
        [SerializeField] private bool autoSpawnPatterns = false; // 스테이지 모드에서는 기본 false
        [SerializeField] private float spawnInterval = 15f; // 패턴 생성 간격 (초)

        private GridManager gridManager;
        private float tickTimer;
        private float spawnTimer;
        private int tickCount = 0;
        private Vector2Int kernelPosition; // 목표 지점
        private Vector2Int playerStartPosition; // 플레이어 시작 위치

        public GridManager Grid => gridManager;
        public float TickRate => tickRate;
        public GameRuleType CurrentStage => currentStage;
        public Vector2Int KernelPosition => kernelPosition;
        public Vector2Int PlayerStartPosition => playerStartPosition;

        void Awake()
        {
            InitializeGrid();
        }

        void Start()
        {
            if (spawnInitialPattern)
            {
                LoadStage(currentStage);
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

                    // 커널과 플레이어 설치 세포는 규칙 적용 제외
                    if (cell.Type == CellType.Kernel || cell.Type == CellType.Placed)
                    {
                        cell.WillBeAlive = cell.IsAlive;
                        continue;
                    }

                    int liveNeighbors = gridManager.CountLiveNeighbors(x, y);

                    // 스테이지별 규칙 적용
                    switch (currentStage)
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
                if (cell.WillBeAlive) cell.Type = CellType.Enemy;
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
                if (cell.WillBeAlive) cell.Type = CellType.Enemy;
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
                if (cell.WillBeAlive) cell.Type = CellType.Enemy;
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
                if (cell.WillBeAlive) cell.Type = CellType.Enemy;
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
                if (cell.WillBeAlive) cell.Type = CellType.Enemy;
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

        public void LoadStage(GameRuleType stage)
        {
            currentStage = stage;
            ClearGrid();

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
            // 스테이지 1: 튜토리얼 - 기본 콘웨이 규칙
            CreateStageBoundary();

            // 플레이어 시작 위치 - 좌하단
            playerStartPosition = new Vector2Int(8, 8);

            // 간단한 미로
            CreateHorizontalPlatform(10, 10, 12);
            CreateHorizontalPlatform(28, 10, 12);
            CreateHorizontalPlatform(15, 18, 20);

            // 초기 패턴
            SpawnGliderPattern(12, 12);
            SpawnBlinkerPattern(30, 15);

            // 커널 (목표 지점) - 우상단
            kernelPosition = new Vector2Int(42, 22);
            gridManager.SetCellAlive(kernelPosition.x, kernelPosition.y, true, CellType.Kernel);
        }

        private void LoadStage2_HighLife()
        {
            // 스테이지 2: HighLife - Replicator 패턴
            CreateStageBoundary();

            // 플레이어 시작 위치
            playerStartPosition = new Vector2Int(7, 10);

            // 복잡한 미로
            CreateHorizontalPlatform(8, 8, 15);
            CreateHorizontalPlatform(27, 8, 15);
            CreateVerticalWall(23, 10, 8);
            CreateBox(15, 14, 8, 3);

            // HighLife 특화 패턴
            SpawnReplicatorPattern(10, 12);
            SpawnPulsarPattern(32, 18);

            // 커널
            kernelPosition = new Vector2Int(40, 20);
            gridManager.SetCellAlive(kernelPosition.x, kernelPosition.y, true, CellType.Kernel);
        }

        private void LoadStage3_Maze()
        {
            // 스테이지 3: Maze - 미로 생성 규칙
            CreateStageBoundary();

            // 플레이어 시작 위치
            playerStartPosition = new Vector2Int(7, 7);

            // 미로 규칙이 자동으로 미로를 생성하므로 초기 씨앗만 배치
            for (int i = 0; i < 5; i++)
            {
                int x = Random.Range(10, 40);
                int y = Random.Range(10, 20);
                CreateBox(x, y, 3, 3);
            }

            // 커널
            kernelPosition = new Vector2Int(43, 23);
            gridManager.SetCellAlive(kernelPosition.x, kernelPosition.y, true, CellType.Kernel);
        }

        private void LoadStage4_DayAndNight()
        {
            // 스테이지 4: Day & Night - 매우 활발한 규칙
            CreateStageBoundary();

            // 플레이어 시작 위치 - 안전 지대 중앙
            playerStartPosition = new Vector2Int(15, 15);

            // 안전 지대 (플레이어용)
            CreateBox(12, 12, 6, 6);

            // 초기 폭발 지점
            SpawnAcornPattern(25, 15);
            SpawnRPentominoPattern(18, 20);

            // 커널
            kernelPosition = new Vector2Int(38, 22);
            gridManager.SetCellAlive(kernelPosition.x, kernelPosition.y, true, CellType.Kernel);
        }

        private void LoadStage5_Seeds()
        {
            // 스테이지 5: Seeds - 폭발형 (최고 난이도)
            CreateStageBoundary();

            // 플레이어 시작 위치
            playerStartPosition = new Vector2Int(25, 22);

            // 플레이어 시작 지점 보호
            CreateBox(23, 20, 4, 4);

            // Seeds 패턴 (2개 이웃이면 탄생)
            SpawnBlinkerPattern(15, 15);
            SpawnBlinkerPattern(35, 15);

            // 커널
            kernelPosition = new Vector2Int(25, 8);
            gridManager.SetCellAlive(kernelPosition.x, kernelPosition.y, true, CellType.Kernel);
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

        private void CreateHorizontalPlatform(int startX, int y, int length)
        {
            for (int x = startX; x < startX + length && x < gridWidth; x++)
            {
                gridManager.SetCellAlive(x, y, true);
            }
        }

        private void CreateVerticalWall(int x, int startY, int height)
        {
            for (int y = startY; y < startY + height && y < gridHeight; y++)
            {
                gridManager.SetCellAlive(x, y, true);
            }
        }

        private void CreateBox(int startX, int startY, int width, int height)
        {
            for (int x = startX; x < startX + width && x < gridWidth; x++)
            {
                for (int y = startY; y < startY + height && y < gridHeight; y++)
                {
                    gridManager.SetCellAlive(x, y, true);
                }
            }
        }

        private void CreateStairs(int startX, int startY, int steps, bool ascending)
        {
            for (int i = 0; i < steps; i++)
            {
                int x = startX + i;
                int y = ascending ? startY + i : startY + (steps - i - 1);

                if (x < gridWidth && y < gridHeight)
                {
                    gridManager.SetCellAlive(x, y, true);
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

        /// <summary>
        /// 특정 위치에 세포를 생성합니다 (우클릭 공격)
        /// </summary>
        public void PlaceCell(int x, int y)
        {
            if (gridManager.IsInBounds(x, y))
            {
                Cell cell = gridManager.GetCell(x, y);
                if (!cell.IsAlive)
                {
                    gridManager.SetCellAlive(x, y, true, CellType.Placed);
                }
            }
        }
    }
}
