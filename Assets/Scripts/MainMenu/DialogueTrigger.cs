using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    // Lớp này dùng để chứa 1 dòng hội thoại
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(3, 10)]
        public string englishLine;
        [TextArea(3, 10)]
        public string vietnameseLine;
    }

    // Kéo SceneLoader vào đây
    public SceneLoader sceneLoader;
    public string nextSceneToLoad;
    public bool startOnAwake = false;

    // Đây là nơi bạn nhập kịch bản vào Inspector
    public DialogueLine[] dialogueLines;

    private DialogueManager manager;

    void Start()
    {
        manager = FindObjectOfType<DialogueManager>();
        if (startOnAwake)
        {
            TriggerDialogue();
        }
    }

    // Hàm này để bắt đầu hội thoại
    public void TriggerDialogue()
    {
        manager.StartDialogue(this);
    }

    // Hàm này được DialogueManager gọi khi nói xong
    public void OnDialogueFinished()
    {
        if (sceneLoader != null && !string.IsNullOrEmpty(nextSceneToLoad))
        {
            sceneLoader.LoadScene(nextSceneToLoad);

        }
    }
}


