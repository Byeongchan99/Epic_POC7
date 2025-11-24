using UnityEngine;
using System.Collections.Generic;

namespace GameOfLife
{
    [System.Serializable]
    public class CellData
    {
        public Vector2Int position;
        public CellType cellType;

        public CellData(Vector2Int pos, CellType type)
        {
            position = pos;
            cellType = type;
        }
    }

    [System.Serializable]
    public class ClusterData
    {
        public Vector2Int corePosition;
        public int normalCellRadius;
        public int normalCellCount;

        public ClusterData(Vector2Int pos, int radius, int count)
        {
            corePosition = pos;
            normalCellRadius = radius;
            normalCellCount = count;
        }
    }

    [CreateAssetMenu(fileName = "StageData", menuName = "Game of Life/Stage Data")]
    public class StageData : ScriptableObject
    {
        [Header("Stage Info")]
        public int stageNumber;
        public string stageName;

        [Header("Life Rules")]
        public string birthRule = "3";
        public string survivalRule = "23";

        [Header("Permanent Walls")]
        public List<CellData> permanentCells = new List<CellData>();

        [Header("Core Clusters")]
        public List<ClusterData> coreClusters = new List<ClusterData>();

        public void Clear()
        {
            permanentCells.Clear();
            coreClusters.Clear();
        }

        public void AddPermanentCell(Vector2Int position)
        {
            permanentCells.Add(new CellData(position, CellType.Permanent));
        }

        public void RemovePermanentCell(Vector2Int position)
        {
            permanentCells.RemoveAll(c => c.position == position);
        }

        public void AddCoreCluster(Vector2Int position, int radius, int count)
        {
            coreClusters.Add(new ClusterData(position, radius, count));
        }

        public void RemoveCoreCluster(Vector2Int position)
        {
            coreClusters.RemoveAll(c => c.corePosition == position);
        }
    }
}
