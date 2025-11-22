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
        Placed      // 플레이어가 설치한 세포
    }
}
