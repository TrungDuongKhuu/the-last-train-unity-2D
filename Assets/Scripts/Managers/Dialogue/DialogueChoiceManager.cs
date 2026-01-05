using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HeneGames.DialogueSystem;
using TMPro;

public class DialogueChoiceManager : MonoBehaviour
{
    // -----------------------------------------------------------------
    // ⭐ SINGLETON
    // -----------------------------------------------------------------
    public static DialogueChoiceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Phát hiện DialogueChoiceManager bị trùng lặp. Hủy object này.");
            Destroy(gameObject);
        }
    }

    // -----------------------------------------------------------------
    // ⭐ DATA CLASS
    // -----------------------------------------------------------------
    [System.Serializable]
    public class Choice
    {
        public string text;
        public int truthImpact;
        public UnityEvent onSelect;
    }

    [System.Serializable]
    public class ChoiceGroup
    {
        public string id;
        public List<Choice> choices;

        [Header("Nếu tick, nhóm này là nhóm cuối → TỰ THOÁT")]
        public bool isFinalGroup = false;
    }

    // -----------------------------------------------------------------
    // ⭐ UI
    // -----------------------------------------------------------------
    [Header("UI")]
    public GameObject choicePanel;
    public Button choiceButtonPrefab;

    // ⭐ List Choice cho scene hiện tại
    [Header("Choice Sets")]
    public List<ChoiceGroup> allChoices;

    // ⭐ DANH SÁCH LỰA CHỌN ĐÃ BỊ XOÁ (theo ID nhóm)
    private Dictionary<string, HashSet<string>> removedChoices = new Dictionary<string, HashSet<string>>();

    // -----------------------------------------------------------------
    private void Start()
    {
        StartCoroutine(InitChoicePanel());
    }

    private IEnumerator InitChoicePanel()
    {
        yield return null;

        if (choicePanel == null)
        {
            choicePanel = GameObject.Find("Choice Panel");
        }

        if (choicePanel != null)
            choicePanel.SetActive(false);
        else
            Debug.LogError("[ChoiceManager] KHÔNG tìm thấy Choice Panel! Hãy gán reference.");
    }

    // -----------------------------------------------------------------
    // ⭐ NẠP DỮ LIỆU CHOICE CHO SCENE
    // -----------------------------------------------------------------
    public void LoadChoicesForScene(List<ChoiceGroup> sceneChoices)
    {
        this.allChoices = sceneChoices;
        Debug.Log($"[ChoiceManager] Đã nạp {sceneChoices.Count} choice group cho scene.");
    }

    // -----------------------------------------------------------------
    // ⭐ EXIT CHOICE
    // -----------------------------------------------------------------
    public void ExitChoice()
    {
        if (choicePanel != null)
            choicePanel.SetActive(false);

        DialogueManager dm = Object.FindFirstObjectByType<DialogueManager>();
        if (dm != null)
            dm.StopDialogue();

        Debug.Log("[ChoiceManager] EXIT CHOICE thành công.");
    }

    // -----------------------------------------------------------------
    // ⭐ SHOW CHOICE
    // -----------------------------------------------------------------
    public void ShowChoices(string id)
    {
        // -------------------------------------------------------------
        // ⭐ DỪNG ĐỐI THOẠI CŨ
        // -------------------------------------------------------------
        if (DialogueUI.instance != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                DialogueManager playerDM = player.GetComponent<DialogueManager>();
                if (playerDM != null)
                    playerDM.StopDialogue();
            }
            else
            {
                DialogueUI.instance.ClearText();
            }
        }

        if (choicePanel == null)
        {
            Debug.LogError("[ChoiceManager] Choice Panel vẫn NULL!");
            return;
        }

        choicePanel.SetActive(true);

        // XÓA CÁC NÚT CŨ
        foreach (Transform child in choicePanel.transform)
            Destroy(child.gameObject);

        // LẤY GROUP
        ChoiceGroup group = allChoices.Find(x => x.id == id);

        if (group == null)
        {
            //Debug.LogError($"[ChoiceManager] Không tìm thấy ChoiceGroup có id: {id}");
            return;
        }

        // ⭐ TẠO HASHSET NẾU CHƯA CÓ
        if (!removedChoices.ContainsKey(id))
            removedChoices[id] = new HashSet<string>();

        // ⭐ Nếu group là FINAL GROUP → tạo nút EXIT
        if (group.isFinalGroup)
        {
            //Debug.Log("[ChoiceManager] FINAL GROUP → tạo nút EXIT.");
            CreateExitButton();
            return;
        }

        // -------------------------------------------------------------
        // ⭐ TẠO NÚT CHOICE
        // -------------------------------------------------------------
        int createdCount = 0;

        foreach (var c in group.choices)
        {
            if (removedChoices[id].Contains(c.text))
                continue;

            createdCount++;

            var btn = Instantiate(choiceButtonPrefab, choicePanel.transform);
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = c.text;

            btn.onClick.AddListener(() =>
            {
                // END = thoát ngay
                if (c.text.Trim().ToUpper() == "END")
                {
                    ExitChoice();
                    return;
                }

                // Ẩn panel
                choicePanel.SetActive(false);

                // Lưu lựa chọn vừa chọn
                removedChoices[id].Add(c.text);

                // ⭐ TĂNG / GIẢM TRUTHFULNESS
                if (TruthfulnessManager.Instance != null)
                    TruthfulnessManager.Instance.AddTruth(c.truthImpact);
                else
                    Debug.LogError("[ChoiceManager] TruthfulnessManager.Instance == NULL!");

                // Gọi event gốc
                c.onSelect.Invoke();
            });
        }

        // -------------------------------------------------------------
        // ⭐ LOGIC — NẾU HẾT LỰA CHỌN → EXIT
        // -------------------------------------------------------------
        if (createdCount == 0)
        {
            Debug.Log("[ChoiceManager] Không còn lựa chọn nào → AUTO EXIT.");

            PlayerPrefs.SetInt("TRIAL_ALL_CLEAR", 1);
            PlayerPrefs.Save();
            PlayerPrefsMonitor.NotifyChanged("TRIAL_ALL_CLEAR");

            ExitChoice();
            return;
        }
    }

    // -----------------------------------------------------------------
    private void CreateExitButton()
    {
        var btn = Instantiate(choiceButtonPrefab, choicePanel.transform);
        btn.GetComponentInChildren<TextMeshProUGUI>().text = "Tôi đã hiểu.";

        btn.onClick.AddListener(() =>
        {
            ExitChoice();
        });
    }

    // -----------------------------------------------------------------
    // ⭐ RESET CHOICE
    // -----------------------------------------------------------------
    public void ResetChoices(string id)
    {
        if (removedChoices.ContainsKey(id))
            removedChoices[id].Clear();
    }
}
