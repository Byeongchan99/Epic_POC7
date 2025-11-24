using UnityEngine;
using UnityEditor;
using GameOfLife;

public class StageEditorWindow : EditorWindow
{
    private StageData currentStageData;
    private GameOfLifeManager gameManager;
    private GridManager gridManager;

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
        gameManager = FindObjectOfType<GameOfLifeManager>();
        gridManager = FindObjectOfType<GridManager>();
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
            "Make sure GameOfLifeManager and GridManager are in the scene",
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
        if (gridManager == null || gameManager == null)
        {
            FindManagers();
            return;
        }

        // Draw grid
        if (showGrid)
        {
            DrawGrid();
        }

        // Handle mouse input
        HandleMouseInput();

        // Repaint scene view
        SceneView.RepaintAll();
    }

    private void DrawGrid()
    {
        Handles.color = gridColor;

        int gridWidth = gridManager.GridWidth;
        int gridHeight = gridManager.GridHeight;

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

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 worldPos = ray.origin;

            // Convert to grid coordinates
            int gridX = Mathf.RoundToInt(worldPos.x);
            int gridY = Mathf.RoundToInt(worldPos.y);

            if (gridManager.IsInBounds(gridX, gridY))
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
            }
        }
    }

    private void PlaceCellAt(int x, int y)
    {
        switch (currentMode)
        {
            case PlacementMode.Permanent:
                gridManager.SetCellAlive(x, y, true, CellType.Permanent);
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
        Cell cell = gridManager.GetCell(x, y);

        if (cell != null && cell.IsAlive)
        {
            gridManager.SetCellAlive(x, y, false);

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
        for (int x = 0; x < gridManager.GridWidth; x++)
        {
            for (int y = 0; y < gridManager.GridHeight; y++)
            {
                Cell cell = gridManager.GetCell(x, y);
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

        if (gameManager == null || gridManager == null)
        {
            Debug.LogWarning("GameOfLifeManager or GridManager not found!");
            return;
        }

        // Clear current grid
        gridManager.ClearGrid();

        // Load permanent cells
        foreach (var cellData in currentStageData.permanentCells)
        {
            gridManager.SetCellAlive(cellData.position.x, cellData.position.y, true, CellType.Permanent);
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
