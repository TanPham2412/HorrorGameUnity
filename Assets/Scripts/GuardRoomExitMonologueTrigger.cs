using UnityEngine;

public class GuardRoomExitMonologueTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public string requiredFlag = "OfficeTapeCompleted";
    public string playerTag = "Player";

    [Header("Monologue")]
    [TextArea(2, 5)]
    public string line = "Đường lên tầng 2 mở rồi. Thật kỳ quái... Cứ như có kẻ nào đó đang dẫn dắt mình từng bước một. Không còn cách nào khác, lên thôi.";
    public float duration = 5f;
    public bool addToLog = true;

    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!StoryFlagManager.HasFlag(requiredFlag)) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;
        MonologueManager.PlayMonologue(line, duration, addToLog, true);
    }
}