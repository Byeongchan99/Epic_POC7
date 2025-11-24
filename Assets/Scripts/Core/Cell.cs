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

        public Cell(int x, int y, bool isAlive = false, CellType type = CellType.Normal)
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

    /// <summary>
    /// 셀의 종류
    /// </summary>
    public enum CellType
    {
        Normal,     // 일반 셀: 공격 O, 생명 로직 O (흰색 적 세포)
        Permanent,  // 영구 셀: 공격 X, 생명 로직 X (검은 회색 벽/맵)
        Core,       // 코어 셀: 공격 O, 생명 로직 X (빨간색 핵심 타겟)
        Player      // 플레이어 셀: 공격 X, 생명 로직 O (초록색 설치 세포)
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
        public Vector2Int kernelPosition; // 이제는 클리어 지점으로만 사용
        public ClusterConfig[] clusters; // 코어 클러스터 설정
    }

    /// <summary>
    /// 셀 클러스터 설정 (코어 + 주변 일반 셀)
    /// </summary>
    [System.Serializable]
    public class ClusterConfig
    {
        public Vector2Int corePosition;  // 코어 위치
        public int normalCellRadius = 5; // 코어 주변 일반 셀 생성 반경
        public int normalCellCount = 20; // 생성할 일반 셀 개수
    }
}
