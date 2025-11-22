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

        [Header("Initial Pattern")]
        [SerializeField] private bool spawnInitialPattern = true;

        private GridManager gridManager;
        private float tickTimer;
        private int tickCount = 0;

        public GridManager Grid => gridManager;
        public float TickRate => tickRate;

        void Awake()
        {
            InitializeGrid();
        }

        void Start()
        {
            if (spawnInitialPattern)
            {
                SpawnMazePattern();
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
        /// 콘웨이의 생명 게임 규칙에 따라 다음 상태를 계산합니다.
        /// </summary>
        private void CalculateNextState()
        {
            for (int x = 0; x < gridManager.Width; x++)
            {
                for (int y = 0; y < gridManager.Height; y++)
                {
                    Cell cell = gridManager.GetCell(x, y);
                    int liveNeighbors = gridManager.CountLiveNeighbors(x, y);

                    // 콘웨이의 생명 게임 규칙
                    if (cell.IsAlive)
                    {
                        // 살아있는 세포
                        if (liveNeighbors <= 1)
                        {
                            // 고립: 주변에 1개 이하 -> 죽음
                            cell.WillBeAlive = false;
                        }
                        else if (liveNeighbors >= 4)
                        {
                            // 과밀: 주변에 4개 이상 -> 죽음
                            cell.WillBeAlive = false;
                        }
                        else
                        {
                            // 2~3개: 생존
                            cell.WillBeAlive = true;
                        }
                    }
                    else
                    {
                        // 죽어있는 세포
                        if (liveNeighbors == 3)
                        {
                            // 탄생: 정확히 3개 -> 새로운 세포 탄생
                            cell.WillBeAlive = true;
                            cell.Type = CellType.Enemy; // 새로 태어난 세포는 적
                        }
                        else
                        {
                            cell.WillBeAlive = false;
                        }
                    }
                }
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

            // 추가 장애물 (동적 패턴 생성용)
            SpawnGliderPattern(centerX - 3, centerY);
            SpawnBlinkerPattern(centerX + 5, centerY + 3);
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
