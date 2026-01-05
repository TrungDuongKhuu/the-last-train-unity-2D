using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject settingsPanel;

    [Header("HUD Buttons")]
    public GameObject pauseBtn;
    public GameObject playBtn;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        settingsPanel.SetActive(false);
    }

    public void ShowHUD(bool show)
    {
        hudPanel.SetActive(show);
    }

    public void ShowPausePanel(bool show)
    {
        pausePanel.SetActive(show);
    }

    public void ShowSettingsPanel(bool show)
    {
        settingsPanel.SetActive(show);
    }

    public void OnSetClicked()
    {
        pauseBtn.SetActive(false);
        playBtn.SetActive(true);
        Time.timeScale = 0f; // Tạm dừng game
        ShowSettingsPanel(true);
    }


    // Xử lý PauseBtn / PlayBtn
    public void OnPauseBtnClicked()
    {
        pauseBtn.SetActive(false);
        playBtn.SetActive(true);
        Time.timeScale = 0f; // Tạm dừng game
        //ShowPausePanel(true);
    }

    public void OnPlayBtnClicked()
    {
        playBtn.SetActive(false);
        pauseBtn.SetActive(true);
        Time.timeScale = 1f; // Tiếp tục game
        //ShowPausePanel(false);
    }
}
