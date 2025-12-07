using UnityEngine;

public class GhostActivator : MonoBehaviour
{
    [Header("Kéo con Ghost vào đây")]
    public GhostBoss ghostAI;

    [Header("Cài đặt")]
    public bool destroyAfterTrigger = true; // Chạm xong có xóa vật này đi không?

    private void OnTriggerEnter(Collider other)
    {
        // Chỉ kích hoạt nếu người chạm là Player
        if (other.CompareTag("Player"))
        {
            if (ghostAI != null)
            {
                Debug.Log("ZME: Đã chạm vào vật kích hoạt! Gọi Ghost dậy...");

                // Gọi hàm kích hoạt trong script AI
                ghostAI.ActivateGhost();
            }
            else
            {
                Debug.LogError("ZME: Bạn quên chưa kéo Ghost vào ô Script rồi!");
            }

            // Xử lý vật phẩm sau khi chạm
            if (destroyAfterTrigger)
            {
                gameObject.SetActive(false); // Ẩn đi
            }
            else
            {
                // Nếu không ẩn thì tắt collider để không kích hoạt lại lần 2
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}