using System;
using Audio;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseMenuController: MonoBehaviour
    {
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private GameObject pauseMenuWindow;
        [SerializeField] private GameObject optionsMenuWindow;

        public static bool IsPaused = false;
        
        public event Action<bool> OnPause;
        
        private void Start()
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;
            IsPaused = false;
        }

        public void Pause(bool menu = true)
        {
            pauseScreen.SetActive(true);
            pauseMenuWindow.SetActive(menu);
            optionsMenuWindow.SetActive(false);
            Time.timeScale = 0f;
            IsPaused = true;
            OnPause?.Invoke(IsPaused);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PauseMusic();
        }

        public void Resume()
        {
            pauseScreen.SetActive(false);
            pauseMenuWindow.SetActive(true);
            optionsMenuWindow.SetActive(false);
            Time.timeScale = 1f;
            IsPaused = false;
            OnPause?.Invoke(IsPaused);

            if (AudioManager.Instance != null)
                AudioManager.Instance.ResumeMusic();
        }

        public void OnInputPause(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            
            if (IsPaused) 
                Resume();
            else 
                Pause();
        }
        
        public void OnButtonResume()
        {
            Resume();
        }
        
        public void OnButtonRestart()
        {
            pauseScreen.SetActive(false);
            pauseMenuWindow.SetActive(true);
            optionsMenuWindow.SetActive(false);
            SokobanGameManager.Instance.RestartLevel();
            Resume();
        }

        public void OnButtonMainMenu()
        {
            if (IsPaused)
                Resume(); // BẮT BUỘC phải unpause trước khi load scene

            // TẮT UI PAUSE TRƯỚC KHI LOAD
            pauseScreen.SetActive(false);
            pauseMenuWindow.SetActive(false);
            optionsMenuWindow.SetActive(false);

            Time.timeScale = 1f; // đảm bảo không bị dừng thời gian

            // QUAY VỀ OFFICE ĐÚNG HỆ THỐNG GAME MANAGER
            GameManager.Instance.LoadSceneWithFade("Office Room", "Enter_FromPuzzle1");
        }

    }
}