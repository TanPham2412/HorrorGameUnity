using UnityEngine;

public class GhostActivator : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Kéo con Ghost vào đây")]
    public GhostAI_Hybrid ghostScript;

    [Tooltip("Nếu tích, Ghost sẽ kích hoạt khi Player chạm vào vùng Trigger")]
    public bool activateOnTriggerEnter = true;

    [Tooltip("Chỉ kích hoạt 1 lần duy nhất")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    // Tự động tìm Ghost nếu quên kéo
    void Start()
    {
        if (ghostScript == null)
        {
            ghostScript = FindObjectOfType<GhostAI_Hybrid>();
        }
    }

    // Dùng cho Trigger Box (đi qua cửa là ma xuất hiện)
    void OnTriggerEnter(Collider other)
    {
        if (activateOnTriggerEnter && other.CompareTag("Player"))
        {
            Activate();
        }
    }

    // Hàm này có thể gọi từ nút bấm hoặc sự kiện nhặt đồ khác
    public void Activate()
    {
        if (triggerOnce && hasTriggered) return;

        if (ghostScript != null)
        {
            ghostScript.ActivateGhost();
            hasTriggered = true;
            Debug.Log("Activator: Đã gọi Ghost dậy!");
        }
    }
}