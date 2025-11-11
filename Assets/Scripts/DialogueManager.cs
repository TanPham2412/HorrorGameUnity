using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Bắt buộc để dùng TextMeshPro

public class DialogueManager : MonoBehaviour
{
    // Kéo UI từ Hierarchy vào đây
    public GameObject dialogueBoxUI;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI englishLineText;
    public TextMeshProUGUI vietnameseLineText;

    private Queue<DialogueTrigger.DialogueLine> lines = new Queue<DialogueTrigger.DialogueLine>(); // Hàng đợi lời thoại
    private PlayerController currentPlayer; // Để khóa/mở player
    private DialogueTrigger currentTrigger; // Để biết ai đã gọi

    void Start()
    {
        //lines = new Queue<DialogueTrigger.DialogueLine>();
        dialogueBoxUI.SetActive(false); // Đảm bảo UI tắt khi bắt đầu
    }

    // Hàm được gọi bởi DialogueTrigger
    public void StartDialogue(DialogueTrigger trigger)
    {
        currentTrigger = trigger; // Lưu lại trigger đã gọi

        // Tìm và khóa Player
        currentPlayer = FindObjectOfType<PlayerController>();
        if (currentPlayer != null)
        {
            currentPlayer.enabled = false;
        }

        // Mở khóa chuột và hiện UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        dialogueBoxUI.SetActive(true);

        lines.Clear(); // Xóa hội thoại cũ

        // Nạp hội thoại mới vào hàng đợi
        foreach (DialogueTrigger.DialogueLine line in trigger.dialogueLines)
        {
            lines.Enqueue(line);
        }

        DisplayNextLine(); // Hiển thị câu đầu tiên
    }

    // Hàm này chạy trong Update()
    public void DisplayNextLine()
    {
        // Nếu không còn lời thoại
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Lấy câu thoại tiếp theo
        DialogueTrigger.DialogueLine currentLine = lines.Dequeue();

        // Hiển thị lên UI
        speakerNameText.text = currentLine.speakerName;
        // Hiển thị cả tiếng Anh và tiếng Việt
        englishLineText.text = currentLine.englishLine;
        vietnameseLineText.text = "<i>" + currentLine.vietnameseLine + "</i>";
    }

    void EndDialogue()
    {
        // Ẩn UI
        dialogueBoxUI.SetActive(false);

        // Mở lại Player và khóa chuột
        if (currentPlayer != null)
        {
            currentPlayer.enabled = true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // BÁO CHO TRIGGER BIẾT ĐÃ NÓI XONG
        // (Để trigger có thể quyết định chuyển cảnh)
        currentTrigger.OnDialogueFinished();
    }

    // Chúng ta sẽ gọi hàm này bằng cách nhấn phím
    void Update()
    {
        // Nếu UI đang bật VÀ người chơi nhấn E
        if (dialogueBoxUI.activeInHierarchy && Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextLine();
        }
    }
}
