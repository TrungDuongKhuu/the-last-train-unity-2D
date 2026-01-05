using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeneGames.DialogueSystem
{
    public class SceneStartMonologue : MonoBehaviour
    {
        [Header("Monologue Configuration")]
        [Tooltip("Delay in seconds before the monologue starts after the scene loads.")]
        [SerializeField] private float startDelay = 0.5f;

        [Header("Monologue Content")]
        [SerializeField] private List<NPC_Sentence> monologueSentences;

        private bool hasTriggered = false;

        // Start is called before the first frame update
        void Start()
        {
            // Bắt đầu coroutine để kích hoạt độc thoại sau một khoảng trễ
            if (!hasTriggered && monologueSentences.Count > 0)
            {
                StartCoroutine(StartMonologueAfterDelay());
            }
        }

        private IEnumerator StartMonologueAfterDelay()
        {
            // Đợi một khoảng thời gian ngắn để đảm bảo mọi thứ đã được tải xong
            yield return new WaitForSeconds(startDelay);

            // Tìm đối tượng người chơi bằng tag "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player == null)
            {
                Debug.LogError("SceneStartMonologue Error: Cannot find GameObject with tag 'Player'. Make sure your player object has the correct tag.");
                yield break;
            }

            // Lấy component DialogueManager từ người chơi
            DialogueManager playerDialogueManager = player.GetComponent<DialogueManager>();

            if (playerDialogueManager == null)
            {
                Debug.LogError("SceneStartMonologue Error: The 'Player' GameObject does not have a DialogueManager component attached.");
                yield break;
            }

            // Đánh dấu là đã kích hoạt để không chạy lại
            hasTriggered = true;

            // Bắt đầu đoạn độc thoại bằng cách gọi hàm công khai chúng ta đã tạo
            playerDialogueManager.StartCustomDialogue(monologueSentences);
        }
    }
}