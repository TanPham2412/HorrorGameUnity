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

    private bool hasLoggedFirstLine;
    private bool hasLoggedSecondLine;

    void Start()
    {
        ThePlayer.GetComponent<FirstPersonController>().enabled = false;
        StartCoroutine(ScenePlayer());
    }

    IEnumerator ScenePlayer()
    {
        yield return new WaitForSeconds(1.5f);
        FadeScreenIn.SetActive(false);

        string firstLine = "Một cơn đau buốt như búa bổ xé toạc tâm trí tôi. Tôi đang ở đâu thế này? Tôi... tôi không nhớ được gì cả.";
        TextBox.GetComponent<TextMeshProUGUI>().text = firstLine;
        TryLogOpeningMonologue(firstLine, ref hasLoggedFirstLine);
        yield return new WaitForSeconds(2);

        string secondLine = "Mọi thứ chỉ là một mảng mờ. Khi tầm nhìn dần rõ lại, thứ đầu tiên tôi thấy là một cuộn băng cassette trên bàn. Có lẽ... tôi nên bắt đầu từ đó.";
        TextBox.GetComponent<TextMeshProUGUI>().text = secondLine;
        TryLogOpeningMonologue(secondLine, ref hasLoggedSecondLine);
        yield return new WaitForSeconds(3);

        TextBox.GetComponent<TextMeshProUGUI>().text = "";
        ThePlayer.GetComponent<FirstPersonController>().enabled = true;
    }

    private void TryLogOpeningMonologue(string text, ref bool hasLogged)
    {
        if (!hasLogged && !string.IsNullOrWhiteSpace(text))
        {
            ClueLogManager.AddMonologue(text);
            hasLogged = true;
        }
    }

}