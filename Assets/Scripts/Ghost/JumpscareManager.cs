using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class JumpscareManager : MonoBehaviour
{
    // Biến tĩnh để gọi từ bất cứ đâu
    public static JumpscareManager instance;

    [Header("Cài đặt")]
    public GameObject jumpscareScreen; // Cái RawImage
    public VideoPlayer videoPlayer;    // Cái Video Player

    void Awake()
    {
        instance = this;
    }

    public void TriggerJumpscare()
    {
        // 1. Bật màn hình đen/video lên
        jumpscareScreen.SetActive(true);

        // 2. Phát video
        videoPlayer.Play();

        // 3. Bắt đầu đếm ngược để reset game
        // (Lấy độ dài của video để chờ)
        StartCoroutine(WaitAndReload(videoPlayer.length));
    }

    IEnumerator WaitAndReload(double videoDuration)
    {
        // Chờ cho đến khi video chạy xong
        // (Cộng thêm 0.5s cho chắc ăn)
        yield return new WaitForSeconds((float)videoDuration + 0.5f);

        // 4. Tải lại màn chơi (Gọi hàm Die của Player hoặc tự load)
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}