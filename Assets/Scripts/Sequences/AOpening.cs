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
    public GameObject TextBox;

    void Start()
    {
        ThePlayer.GetComponent<FirstPersonController>().enabled = false;
        StartCoroutine(ScenePlayer());
    }

    IEnumerator ScenePlayer()
    {
        yield return new WaitForSeconds(1.5f);
        FadeScreenIn.SetActive(false);
        TextBox.GetComponent<TextMeshProUGUI>().text = "Một cơn đau buốt như búa bổ xé toạc tâm trí tôi. Tôi đang ở đâu thế này? Tôi... tôi không nhớ được gì cả.";
        yield return new WaitForSeconds(2);
        TextBox.GetComponent<TextMeshProUGUI>().text = "Mọi thứ chỉ là một mảng mờ. Khi tầm nhìn dần rõ lại, thứ đầu tiên tôi thấy là một cuộn băng cassette trên bàn. Có lẽ... tôi nên bắt đầu từ đó.";
        yield return new WaitForSeconds(3);
        TextBox.GetComponent<TextMeshProUGUI>().text = "";
        ThePlayer.GetComponent<FirstPersonController>().enabled = true;
    }

}