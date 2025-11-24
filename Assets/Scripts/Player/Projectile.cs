using UnityEngine;
using GameOfLife.Manager;
using GameOfLife.Core;

namespace GameOfLife.Player
{
    /// <summary>
    /// 플레이어가 발사하는 투사체(총알)입니다.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float damageRadius = 0.3f; // 총알이 셀을 파괴할 반경

        private Vector3 direction;
        private GameOfLifeManager gameManager;
        private float aliveTime = 0f;

        public void Initialize(Vector3 direction, GameOfLifeManager manager)
        {
            this.direction = direction.normalized;
            this.gameManager = manager;
        }

        void Update()
        {
            // 이동
            transform.position += direction * speed * Time.deltaTime;

            // 수명 체크
            aliveTime += Time.deltaTime;
            if (aliveTime >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // 셀 충돌 체크
            CheckCellCollision();
        }

        private void CheckCellCollision()
        {
            if (gameManager == null || gameManager.Grid == null) return;

            Vector2Int gridPos = gameManager.Grid.WorldToGridPosition(transform.position);

            // 현재 위치 주변의 셀들을 확인
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int x = gridPos.x + dx;
                    int y = gridPos.y + dy;

                    if (gameManager.Grid.IsInBounds(x, y))
                    {
                        Cell cell = gameManager.Grid.GetCell(x, y);
                        if (cell != null && cell.IsAlive)
                        {
                            // Permanent 셀은 파괴 불가
                            if (cell.Type == CellType.Permanent)
                                continue;

                            Vector3 cellWorldPos = gameManager.Grid.GridToWorldPosition(x, y);
                            float distance = Vector3.Distance(transform.position, cellWorldPos);

                            if (distance <= damageRadius)
                            {
                                // 코어 파괴 시 주변 셀 연쇄 파괴
                                if (cell.Type == CellType.Core)
                                {
                                    gameManager.DestroyCoreAndSurroundingCells(x, y, 10);
                                    Debug.Log($"Core destroyed! Chained destruction at ({x}, {y})");
                                }
                                else
                                {
                                    // 일반 셀 또는 플레이어 셀만 파괴
                                    gameManager.DeleteCell(x, y);
                                }

                                // 총알 파괴
                                Destroy(gameObject);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
