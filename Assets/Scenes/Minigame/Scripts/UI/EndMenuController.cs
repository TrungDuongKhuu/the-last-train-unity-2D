using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class EndMenuController: MonoBehaviour
    {
        [SerializeField] private GameObject endMenuWindow;
        [SerializeField] private int mainMenuSceneIndex = 0;
        
        private void Start()
        {
            endMenuWindow.SetActive(false);
        }
        
        public void DisplayEndMenu()
        {
            endMenuWindow.SetActive(true);
        }

        //public void OnButtonNextLevel()
        //{
        //    endMenuWindow.SetActive(false);
        //    if (SokobanGameManager.Instance.HasNextLevel)
        //        SokobanGameManager.Instance.LoadNextLevel();
        //    else
        //        SceneManager.LoadScene(mainMenuSceneIndex);
            
        //    SokobanGameManager.Instance.ResumeGame();
        //}

        public void OnButtonNextLevel()
        {
            if (SokobanGameManager.Instance.HasNextLevel)
                SokobanGameManager.Instance.LoadNextLevel();
            else
                GameManager.Instance.LoadSceneWithFade("Office Room", "Enter_FromPuzzle1");
        }


        public void OnButtonRestartLevel()
        {
            endMenuWindow.SetActive(false);
            SokobanGameManager.Instance.RestartLevel();
            SokobanGameManager.Instance.ResumeGame();
        }
    }
}