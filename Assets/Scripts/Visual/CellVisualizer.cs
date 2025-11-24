using UnityEngine;
using GameOfLife.Core;
using GameOfLife.Manager;

namespace GameOfLife.Visual
{
    /// <summary>
    /// 세포들을 화면에 시각화합니다.
    /// Unity의 SpriteRenderer를 사용하여 각 셀을 정사각형으로 표현합니다.
    /// </summary>
    public class CellVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameOfLifeManager gameManager;

        [Header("Visual Settings")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Color normalColor = Color.white;      // 일반 셀 (흰색)
        [SerializeField] private Color permanentColor = new Color(0.2f, 0.2f, 0.2f); // 영구 셀/벽 (검은 회색)
        [SerializeField] private Color coreColor = new Color(1f, 0.2f, 0.2f); // 코어 (밝은 빨간색)
        [SerializeField] private Color playerColor = new Color(0.2f, 1f, 0.2f); // 플레이어 셀 (초록색)
        [SerializeField] private Color previewColor = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 예측 (희미한 회색)
        [SerializeField] private bool showPreview = true;

        private SpriteRenderer[,] cellRenderers;

        void Start()
        {
            Debug.Log("CellVisualizer Start() called");

            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameOfLifeManager>();
                Debug.Log($"CellVisualizer: Found GameManager = {gameManager != null}");
            }

            if (gameManager == null)
            {
                Debug.LogError("CellVisualizer: GameOfLifeManager not found!");
                return;
            }

            if (gameManager.Grid == null)
            {
                Debug.LogError("CellVisualizer: GameOfLifeManager.Grid is null! Retrying in next frame...");
                StartCoroutine(InitializeDelayed());
                return;
            }

            if (cellPrefab == null)
            {
                Debug.Log("CellVisualizer: Creating default cell prefab");
                CreateDefaultCellPrefab();
            }

            InitializeVisuals();
            Debug.Log($"CellVisualizer initialized with {gameManager.Grid.Width}x{gameManager.Grid.Height} grid");
        }

        private System.Collections.IEnumerator InitializeDelayed()
        {
            // 다음 프레임까지 대기 (GameOfLifeManager의 Awake가 실행될 때까지)
            yield return null;

            if (gameManager.Grid == null)
            {
                Debug.LogError("CellVisualizer: GameOfLifeManager.Grid is still null after delay!");
                yield break;
            }

            if (cellPrefab == null)
            {
                Debug.Log("CellVisualizer: Creating default cell prefab (delayed)");
                CreateDefaultCellPrefab();
            }

            InitializeVisuals();
            Debug.Log($"CellVisualizer initialized (delayed) with {gameManager.Grid.Width}x{gameManager.Grid.Height} grid");
        }

        void LateUpdate()
        {
            UpdateVisuals();
        }

        private void CreateDefaultCellPrefab()
        {
            // 기본 셀 프리팹 생성 (SpriteRenderer with Sprite)
            cellPrefab = new GameObject("CellPrefab");
            SpriteRenderer sr = cellPrefab.AddComponent<SpriteRenderer>();

            // 흰색 정사각형 스프라이트 생성
            sr.sprite = CreateSquareSprite();
            sr.color = Color.white;

            cellPrefab.transform.localScale = Vector3.one * gameManager.Grid.CellSize * 0.9f;

            // 프리팹을 씬에서 숨김 (하이어라키에 표시되지 않음)
            cellPrefab.hideFlags = HideFlags.HideInHierarchy;
            cellPrefab.SetActive(false);
        }

        private Sprite CreateSquareSprite()
        {
            // 1x1 흰색 텍스처 생성
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        }

        private void InitializeVisuals()
        {
            GridManager grid = gameManager.Grid;
            cellRenderers = new SpriteRenderer[grid.Width, grid.Height];

            Debug.Log($"InitializeVisuals: Creating {grid.Width * grid.Height} cell objects...");
            int createdCount = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Vector3 worldPos = grid.GridToWorldPosition(x, y);
                    GameObject cellObj = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                    cellObj.name = $"Cell_{x}_{y}";

                    // Instantiate된 오브젝트는 Hierarchy에 보이도록 설정
                    cellObj.hideFlags = HideFlags.None;
                    cellObj.SetActive(true);

                    SpriteRenderer sr = cellObj.GetComponent<SpriteRenderer>();
                    cellRenderers[x, y] = sr;
                    sr.enabled = false; // 초기에는 비활성화
                    createdCount++;
                }
            }

            Debug.Log($"InitializeVisuals: Successfully created {createdCount} cell objects under {transform.name}");
        }

        private void UpdateVisuals()
        {
            if (gameManager == null || gameManager.Grid == null || cellRenderers == null)
                return;

            GridManager grid = gameManager.Grid;
            int aliveCellCount = 0;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Cell cell = grid.GetCell(x, y);
                    SpriteRenderer sr = cellRenderers[x, y];

                    if (cell.IsAlive)
                    {
                        sr.enabled = true;
                        sr.color = GetColorForCell(cell);
                        aliveCellCount++;
                    }
                    else if (showPreview && cell.WillBeAlive)
                    {
                        // 다음 턴에 태어날 세포 미리보기
                        sr.enabled = true;
                        sr.color = previewColor;
                    }
                    else
                    {
                        sr.enabled = false;
                    }
                }
            }

            // 첫 몇 프레임만 로그 출력
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"UpdateVisuals: {aliveCellCount} cells alive");
            }
        }

        private Color GetColorForCell(Cell cell)
        {
            switch (cell.Type)
            {
                case CellType.Normal:
                    return normalColor;
                case CellType.Permanent:
                    return permanentColor;
                case CellType.Core:
                    return coreColor;
                case CellType.Player:
                    return playerColor;
                default:
                    return Color.white;
            }
        }

        void OnDrawGizmos()
        {
            if (gameManager == null || gameManager.Grid == null) return;

            // 그리드 선 그리기 (에디터에서만 표시)
            GridManager grid = gameManager.Grid;
            Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);

            float width = grid.Width * grid.CellSize;
            float height = grid.Height * grid.CellSize;
            float startX = -width / 2f;
            float startY = -height / 2f;

            // 세로선
            for (int x = 0; x <= grid.Width; x++)
            {
                float xPos = startX + x * grid.CellSize;
                Gizmos.DrawLine(
                    new Vector3(xPos, startY, 0),
                    new Vector3(xPos, startY + height, 0)
                );
            }

            // 가로선
            for (int y = 0; y <= grid.Height; y++)
            {
                float yPos = startY + y * grid.CellSize;
                Gizmos.DrawLine(
                    new Vector3(startX, yPos, 0),
                    new Vector3(startX + width, yPos, 0)
                );
            }
        }
    }
}
