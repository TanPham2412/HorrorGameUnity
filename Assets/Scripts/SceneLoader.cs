using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có dòng này

public class SceneLoader : MonoBehaviour
{
    // Hàm public để Button hoặc script khác gọi
    public void LoadScene(string sceneName)
    {
        // Bật lại Time.timeScale nếu nó bị dừng
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}