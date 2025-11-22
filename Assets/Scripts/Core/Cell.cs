using UnityEngine;

namespace GameOfLife.Core
{
    /// <summary>
    /// 콘웨이의 생명 게임에서 하나의 셀(세포)을 나타냅니다.
    /// </summary>
    public class Cell
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsAlive { get; set; }
        public bool WillBeAlive { get; set; } // 다음 턴에 살아있을지 예측
        public CellType Type { get; set; }

        public Cell(int x, int y, bool isAlive = false, CellType type = CellType.Enemy)
        {
            X = x;
            Y = y;
            IsAlive = isAlive;
            WillBeAlive = isAlive;
            Type = type;
        }

        public void ApplyNextState()
        {
            IsAlive = WillBeAlive;
        }
    }

    public enum CellType
    {
        Enemy,      // 흰색 세포 (적)
        Player,     // 붉은색 플레이어
        Placed,     // 플레이어가 설치한 세포
        Kernel      // 목표 지점 (스테이지 클리어)
    }

    /// <summary>
    /// 생명 게임 규칙 변형
    /// </summary>
    public enum GameRuleType
    {
        ConwayLife,     // 기본 콘웨이 (B3/S23)
        HighLife,       // Replicator 패턴 (B36/S23)
        Maze,           // 미로 생성 (B3/S12345)
        DayAndNight,    // 대칭 규칙 (B3678/S34678)
        Seeds           // 폭발형 (B2/S)
    }

    /// <summary>
    /// 스테이지 설정 정보
    /// </summary>
    [System.Serializable]
    public class StageConfig
    {
        public GameRuleType ruleType;
        public Vector2Int playerStartPosition;
        public Vector2Int kernelPosition;
    }
}
