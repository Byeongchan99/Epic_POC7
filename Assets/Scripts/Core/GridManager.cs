using UnityEngine;
using System.Collections.Generic;

namespace GameOfLife.Core
{
    /// <summary>
    /// 게임 월드의 격자(Grid)를 관리합니다.
    /// </summary>
    public class GridManager
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float CellSize { get; private set; }

        private Cell[,] grid;

        public GridManager(int width, int height, float cellSize = 1f)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;

            InitializeGrid();
        }

        private void InitializeGrid()
        {
            grid = new Cell[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    grid[x, y] = new Cell(x, y);
                }
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (IsInBounds(x, y))
                return grid[x, y];
            return null;
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public Vector3 GridToWorldPosition(int x, int y)
        {
            float offsetX = -(Width * CellSize) / 2f + CellSize / 2f;
            float offsetY = -(Height * CellSize) / 2f + CellSize / 2f;
            return new Vector3(x * CellSize + offsetX, y * CellSize + offsetY, 0);
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPos)
        {
            float offsetX = (Width * CellSize) / 2f;
            float offsetY = (Height * CellSize) / 2f;

            int x = Mathf.FloorToInt((worldPos.x + offsetX) / CellSize);
            int y = Mathf.FloorToInt((worldPos.y + offsetY) / CellSize);

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 주변 8개 셀의 살아있는 이웃 수를 계산합니다.
        /// </summary>
        public int CountLiveNeighbors(int x, int y)
        {
            int count = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (IsInBounds(nx, ny) && grid[nx, ny].IsAlive)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public List<Cell> GetAllCells()
        {
            List<Cell> cells = new List<Cell>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    cells.Add(grid[x, y]);
                }
            }
            return cells;
        }

        public void SetCellAlive(int x, int y, bool alive, CellType type = CellType.Normal)
        {
            if (IsInBounds(x, y))
            {
                grid[x, y].IsAlive = alive;
                grid[x, y].Type = type;
            }
        }

        public void ClearGrid()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    grid[x, y].IsAlive = false;
                    grid[x, y].Type = CellType.Normal;
                }
            }
        }
    }
}
