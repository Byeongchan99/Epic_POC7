using UnityEngine;
using UnityEngine.UI;
using GameOfLife.Manager;
using GameOfLife.Core;

namespace GameOfLife.UI
{
    /// <summary>
    /// 메인 메뉴 UI를 관리하고 스테이지 선택 기능을 제공합니다.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameOfLifeManager gameManager;
        [SerializeField] private GameObject menuPanel;

        [Header("Stage Buttons")]
        [SerializeField] private Button stage1Button;
        [SerializeField] private Button stage2Button;
        [SerializeField] private Button stage3Button;
        [SerializeField] private Button stage4Button;
        [SerializeField] private Button stage5Button;

        [Header("Settings")]
        [SerializeField] private bool showMenuOnStart = true;

        void Start()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameOfLifeManager>();
            }

            SetupButtons();

            if (showMenuOnStart)
            {
                ShowMenu();
            }
            else
            {
                HideMenu();
            }
        }

        void Update()
        {
            // ESC 키로 메뉴 토글
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleMenu();
            }
        }

        private void SetupButtons()
        {
            if (stage1Button != null)
                stage1Button.onClick.AddListener(() => StartStage(0));

            if (stage2Button != null)
                stage2Button.onClick.AddListener(() => StartStage(1));

            if (stage3Button != null)
                stage3Button.onClick.AddListener(() => StartStage(2));

            if (stage4Button != null)
                stage4Button.onClick.AddListener(() => StartStage(3));

            if (stage5Button != null)
                stage5Button.onClick.AddListener(() => StartStage(4));
        }

        private void StartStage(int stageIndex)
        {
            if (gameManager == null)
            {
                Debug.LogError("GameOfLifeManager not found!");
                return;
            }

            Debug.Log($"Starting stage {stageIndex + 1}");
            gameManager.LoadStageByIndex(stageIndex);
            HideMenu();

            // 게임 시작
            Time.timeScale = 1f;
        }

        public void ShowMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
                Time.timeScale = 0f; // 게임 일시정지
            }
        }

        public void HideMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
                Time.timeScale = 1f; // 게임 재개
            }
        }

        public void ToggleMenu()
        {
            if (menuPanel != null)
            {
                if (menuPanel.activeSelf)
                {
                    HideMenu();
                }
                else
                {
                    ShowMenu();
                }
            }
        }

        public void QuitGame()
        {
            Debug.Log("Quitting game...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
