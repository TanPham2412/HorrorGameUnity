using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour
{
    public void Die()
    {
        UnityEngine.Debug.Log("PLAYER ĐÃ CHẾT! Tải lại màn chơi...");
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}