using UnityEngine;
using UnityEditor;
using GameOfLife;
using GameOfLife.Core;
using GameOfLife.Manager;

public class StageEditorWindow : EditorWindow
{
    private StageData currentStageData;
    private GameOfLifeManager gameManager;

    // Placement settings
    private enum PlacementMode { Permanent, Core, Erase }
    private PlacementMode currentMode = PlacementMode.Permanent;

    // Core cluster settings
    private int coreRadius = 5;
    private int coreNormalCellCount = 20;

    // Grid visualization
    private bool showGrid = true;
    private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    [MenuItem("Tools/Stage Editor")]
    public static void ShowWindow()
    {
        GetWindow<StageEditorWindow>("Stage Editor");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        FindManagers();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void FindManagers()
    {
        gameManager = Object.FindFirstObjectByType<GameOfLifeManager>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Stage Editor", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // Stage Data
        EditorGUILayout.LabelField("Current Stage Data", EditorStyles.boldLabel);
        StageData newStageData = (StageData)EditorGUILayout.ObjectField("Stage Data", currentStageData, typeof(StageData), false);

        if (newStageData != currentStageData)
        {
            currentStageData = newStageData;
            if (currentStageData != null)
            {
                LoadStageData();
            }
        }

        EditorGUILayout.Space();

        // Create new stage data button
        if (GUILayout.Button("Create New Stage Data"))
        {
            CreateNewStageData();
        }

        EditorGUILayout.Space();

        // Placement Mode
        EditorGUILayout.LabelField("Placement Mode", EditorStyles.boldLabel);
        currentMode = (PlacementMode)EditorGUILayout.EnumPopup("Mode", currentMode);

        EditorGUILayout.Space();

        // Core cluster settings (only show when in Core mode)
        if (currentMode == PlacementMode.Core)
        {
            EditorGUILayout.LabelField("Core Cluster Settings", EditorStyles.boldLabel);
            coreRadius = EditorGUILayout.IntSlider("Radius", coreRadius, 1, 10);
            coreNormalCellCount = EditorGUILayout.IntSlider("Normal Cell Count", coreNormalCellCount, 5, 50);
        }

        EditorGUILayout.Space();

        // Grid visualization
        EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);
        showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);

        EditorGUILayout.Space();

        // Save/Load buttons
        EditorGUI.BeginDisabledGroup(currentStageData == null);

        if (GUILayout.Button("Save Current Layout to Stage Data"))
        {
            SaveStageData();
        }

        if (GUILayout.Button("Load Stage Data to Scene"))
        {
            LoadStageData();
        }

        if (GUILayout.Button("Clear Stage Data"))
        {
            ClearStageData();
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        // Instructions
        EditorGUILayout.HelpBox(
            "Left Click: Place cell/core\n" +
            "Shift + Left Click: Remove cell/core\n" +
            "Make sure GameOfLifeManager is in the scene",
            MessageType.Info
        );

        // Show stats if stage data exists
        if (currentStageData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stage Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Permanent Cells: {currentStageData.permanentCells.Count}");
            EditorGUILayout.LabelField($"Core Clusters: {currentStageData.coreClusters.Count}");
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        // Play Mode에서는 동작하지 않음
        if (EditorApplication.isPlaying)
            return;

        if (gameManager == null || gameManager.Grid == null)
        {
            FindManagers();
            if (gameManager == null)
            {
                // GameOfLifeManager를 찾지 못하면 Scene View에 경고 표시
                Handles.BeginGUI();
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                GUILayout.Label("GameOfLifeManager not found in scene!", EditorStyles.boldLabel);
                GUILayout.Label("Please open a scene with GameOfLifeManager.");
                GUILayout.EndArea();
                Handles.EndGUI();
                return;
            }
        }

        // Draw grid
        if (showGrid)
        {
            DrawGrid();
        }

        // Handle mouse input - 이벤트를 먼저 캡처
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);

        HandleMouseInput();

        // Repaint scene view
        if (Event.current.type == EventType.MouseDown)
        {
            SceneView.RepaintAll();
        }
    }

    private void DrawGrid()
    {
        Handles.color = gridColor;

        int gridWidth = gameManager.Grid.Width;
        int gridHeight = gameManager.Grid.Height;

        // Draw vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(x, 0, 0);
            Vector3 end = new Vector3(x, gridHeight, 0);
            Handles.DrawLine(start, end);
        }

        // Draw horizontal lines
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = new Vector3(0, y, 0);
            Vector3 end = new Vector3(gridWidth, y, 0);
            Handles.DrawLine(start, end);
        }
    }

    private void HandleMouseInput()
    {
        Event e = Event.current;

        // Layout 단계에서는 처리하지 않음
        if (e.type == EventType.Layout)
            return;

        // 마우스 다운 이벤트만 처리
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            // Scene View의 카메라를 통해 월드 좌표 얻기
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            // Z=0 평면과의 교차점 계산 (2D 게임이므로)
            float enter = 0.0f;
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

            Vector3 worldPos;
            if (groundPlane.Raycast(ray, out enter))
            {
                worldPos = ray.GetPoint(enter);
            }
            else
            {
                // Plane과 교차하지 않으면 ray origin 사용
                worldPos = ray.origin;
                worldPos.z = 0;
            }

            // 그리드 좌표로 변환
            int gridX = Mathf.RoundToInt(worldPos.x);
            int gridY = Mathf.RoundToInt(worldPos.y);

            Debug.Log($"Clicked at world: {worldPos}, grid: ({gridX}, {gridY})");

            if (gameManager.Grid.IsInBounds(gridX, gridY))
            {
                if (e.shift)
                {
                    // Erase mode
                    RemoveCellAt(gridX, gridY);
                }
                else
                {
                    // Place mode
                    PlaceCellAt(gridX, gridY);
                }

                e.Use();
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning($"Grid position ({gridX}, {gridY}) is out of bounds!");
            }
        }
    }

    private void PlaceCellAt(int x, int y)
    {
        switch (currentMode)
        {
            case PlacementMode.Permanent:
                gameManager.Grid.SetCellAlive(x, y, true, CellType.Permanent);
                if (currentStageData != null)
                {
                    currentStageData.AddPermanentCell(new Vector2Int(x, y));
                    EditorUtility.SetDirty(currentStageData);
                }
                Debug.Log($"Placed Permanent cell at ({x}, {y})");
                break;

            case PlacementMode.Core:
                // Place core cluster
                gameManager.CreateCluster(new ClusterConfig
                {
                    corePosition = new Vector2Int(x, y),
                    normalCellRadius = coreRadius,
                    normalCellCount = coreNormalCellCount
                });
                if (currentStageData != null)
                {
                    currentStageData.AddCoreCluster(new Vector2Int(x, y), coreRadius, coreNormalCellCount);
                    EditorUtility.SetDirty(currentStageData);
                }
                Debug.Log($"Placed Core cluster at ({x}, {y}) with radius {coreRadius}");
                break;
        }
    }

    private void RemoveCellAt(int x, int y)
    {
        Cell cell = gameManager.Grid.GetCell(x, y);

        if (cell != null && cell.IsAlive)
        {
            gameManager.Grid.SetCellAlive(x, y, false);

            if (currentStageData != null)
            {
                Vector2Int pos = new Vector2Int(x, y);

                // Try to remove as permanent cell
                currentStageData.RemovePermanentCell(pos);

                // Try to remove as core cluster
                currentStageData.RemoveCoreCluster(pos);

                EditorUtility.SetDirty(currentStageData);
            }

            Debug.Log($"Removed cell at ({x}, {y})");
        }
    }

    private void CreateNewStageData()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Stage Data",
            "NewStageData",
            "asset",
            "Create a new stage data file"
        );

        if (!string.IsNullOrEmpty(path))
        {
            StageData newData = CreateInstance<StageData>();
            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.SaveAssets();
            currentStageData = newData;
            Debug.Log($"Created new stage data at {path}");
        }
    }

    private void SaveStageData()
    {
        if (currentStageData == null)
        {
            Debug.LogWarning("No stage data selected!");
            return;
        }

        // Clear existing data
        currentStageData.Clear();

        // Save all cells from the grid
        for (int x = 0; x < gameManager.Grid.Width; x++)
        {
            for (int y = 0; y < gameManager.Grid.Height; y++)
            {
                Cell cell = gameManager.Grid.GetCell(x, y);
                if (cell != null && cell.IsAlive)
                {
                    if (cell.Type == CellType.Permanent)
                    {
                        currentStageData.AddPermanentCell(new Vector2Int(x, y));
                    }
                    else if (cell.Type == CellType.Core)
                    {
                        // Note: We save core cells, but radius info is lost
                        // Users should use the Core placement mode to maintain radius info
                        currentStageData.AddCoreCluster(new Vector2Int(x, y), 5, 20);
                    }
                }
            }
        }

        EditorUtility.SetDirty(currentStageData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Saved stage data: {currentStageData.permanentCells.Count} permanent cells, {currentStageData.coreClusters.Count} core clusters");
    }

    private void LoadStageData()
    {
        if (currentStageData == null)
        {
            Debug.LogWarning("No stage data selected!");
            return;
        }

        if (gameManager == null || gameManager.Grid == null)
        {
            Debug.LogWarning("GameOfLifeManager not found!");
            return;
        }

        // Clear current grid
        gameManager.Grid.ClearGrid();

        // Load permanent cells
        foreach (var cellData in currentStageData.permanentCells)
        {
            gameManager.Grid.SetCellAlive(cellData.position.x, cellData.position.y, true, CellType.Permanent);
        }

        // Load core clusters
        foreach (var clusterData in currentStageData.coreClusters)
        {
            gameManager.CreateCluster(new ClusterConfig
            {
                corePosition = clusterData.corePosition,
                normalCellRadius = clusterData.normalCellRadius,
                normalCellCount = clusterData.normalCellCount
            });
        }

        Debug.Log($"Loaded stage data: {currentStageData.permanentCells.Count} permanent cells, {currentStageData.coreClusters.Count} core clusters");
    }

    private void ClearStageData()
    {
        if (currentStageData == null) return;

        if (EditorUtility.DisplayDialog("Clear Stage Data",
            "Are you sure you want to clear all data from this stage?",
            "Yes", "No"))
        {
            currentStageData.Clear();
            EditorUtility.SetDirty(currentStageData);
            AssetDatabase.SaveAssets();
            Debug.Log("Cleared stage data");
        }
    }
}
