using UnityEngine;
using HeneGames.DialogueSystem; // Phải using namespace này

public class ChoiceEventHandler : MonoBehaviour
{
    // Hàm này bạn đã có, dùng cho On Select (GIỮ NGUYÊN)
    public void StartDialogueBranch(DialogueManager branchToStart)
    {
        if (DialogueUI.instance != null)
        {
            DialogueUI.instance.StartDialogue(branchToStart);
        }
    }

    /// <summary>
    /// Hàm này là "trung gian" để gọi ShowChoices trên Singleton.
    /// Nó sẽ kích hoạt logic DỪNG ĐỐI THOẠI bên trong DialogueChoiceManager.
    /// </summary>
    public void TriggerShowChoices(string choiceId)
    {
        if (DialogueChoiceManager.Instance != null)
        {
            // Dùng Singleton "xịn" để gọi hàm. Logic dừng đối thoại đã nằm bên trong hàm này.
            DialogueChoiceManager.Instance.ShowChoices(choiceId);
        }
        else
        {
            Debug.LogError("DialogueChoiceManager.Instance is null! Không thể hiện choice.");
        }
    }

    // ⭐ ĐÃ XÓA: Hàm StopCurrentDialogue() không còn cần thiết. ⭐
    // ...
}