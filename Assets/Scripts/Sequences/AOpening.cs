using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AOpening : MonoBehaviour
{

    public GameObject ThePlayer;
    public GameObject FadeScreenIn;
    [Tooltip("Legacy reference - MonologueManager now handles displaying text")] public GameObject TextBox;

    void Start()
    {
        ThePlayer.GetComponent<FirstPersonController>().enabled = false;
        StartCoroutine(ScenePlayer());
    }

    IEnumerator ScenePlayer()
    {
        yield return new WaitForSeconds(1.5f);
        FadeScreenIn.SetActive(false);

        QueueOpeningMonologues();

        yield return new WaitForSeconds(4 + 4); // Tổng thời gian hiển thị hai câu

        ThePlayer.GetComponent<FirstPersonController>().enabled = true;
    }

    private void QueueOpeningMonologues()
    {
        string firstLine = "Một cơn đau buốt như búa bổ xé toạc tâm trí tôi. Tôi đang ở đâu thế này? Tôi... tôi không nhớ được gì cả.";
        MonologueManager.PlayMonologue(firstLine, 4f, addToLog: true, preventDuplicate: true);

        string secondLine = "Mọi thứ chỉ là một mảng mờ. Khi tầm nhìn dần rõ lại, thứ đầu tiên tôi thấy là một cuộn băng cassette trên bàn. Có lẽ... tôi nên bắt đầu từ đó.";
        MonologueManager.PlayMonologue(secondLine, 4f, addToLog: true, preventDuplicate: true);
    }

}