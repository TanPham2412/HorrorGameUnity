using UnityEngine;

public class PrincipalActivationTrigger : MonoBehaviour
{
    [Header("Kéo ông Hiệu Trưởng vào đây")]
    public PrincipalBoss principalBoss;

    [Header("Âm thanh hù dọa (Tùy chọn)")]
    public AudioSource soundSource; // Kéo loa vào đây nếu muốn phát tiếng

    // Biến để đảm bảo chỉ kích hoạt 1 lần
    private bool hasActivated = false;

    void OnTriggerEnter(Collider other)
    {
        // Chỉ kích hoạt khi chưa từng kích hoạt VÀ người chạm vào là Player
        if (!hasActivated && other.CompareTag("Player"))
        {
            if (principalBoss != null)
            {
                // GỌI HÀM BẠN VỪA VIẾT
                principalBoss.ActivatePrincipal();

                // Đánh dấu đã xong
                hasActivated = true;

                // Phát âm thanh (nếu có)
                if (soundSource != null) soundSource.Play();

                // Tùy chọn: Xóa luôn cái trigger này cho nhẹ game
                // Destroy(gameObject, 2f); 
            }
            else
            {
                Debug.LogError("ZME: Quên chưa kéo PrincipalBoss vào Trigger rồi!");
            }
        }
    }
}