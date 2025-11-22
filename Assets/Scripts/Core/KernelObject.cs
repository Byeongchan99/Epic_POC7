using UnityEngine;

namespace GameOfLife.Core
{
    /// <summary>
    /// 스테이지의 목표 지점(커널)을 나타내는 오브젝트입니다.
    /// 게임 오브 라이프 규칙의 영향을 받지 않는 독립된 오브젝트입니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class KernelObject : MonoBehaviour
    {
        [SerializeField] private Color kernelColor = Color.green;
        [SerializeField] private Vector2 size = new Vector2(1f, 1f);

        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer.sprite == null)
            {
                CreateDefaultSprite();
            }

            spriteRenderer.color = kernelColor;
        }

        private void CreateDefaultSprite()
        {
            // 1x1 흰색 텍스처 생성
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// 커널의 위치를 설정합니다.
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// 커널의 색상을 설정합니다.
        /// </summary>
        public void SetColor(Color color)
        {
            kernelColor = color;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }
}
