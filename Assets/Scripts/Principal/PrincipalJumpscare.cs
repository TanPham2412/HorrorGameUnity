using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class PrincipalJumpscare : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip jumpscareClip;
    public GameObject jumpscareUI;

    [HideInInspector] public PrincipalBoss bossController;

    private bool isJumpscaring = false;

    void Start()
    {
        if (videoPlayer)
        {
            videoPlayer.targetCameraAlpha = 0;
            videoPlayer.Stop();
        }

        if (jumpscareUI)
            jumpscareUI.SetActive(false);
    }

    public void TriggerPrincipalJumpscare()
    {
        if (!isJumpscaring)
            StartCoroutine(JumpscareRoutine());
    }

    IEnumerator JumpscareRoutine()
    {
        isJumpscaring = true;
        Time.timeScale = 0;

        if (videoPlayer && jumpscareClip)
        {
            videoPlayer.clip = jumpscareClip;
            jumpscareUI?.SetActive(true);

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.targetCameraAlpha = 1;
            videoPlayer.Play();

            yield return new WaitForSecondsRealtime((float)videoPlayer.length);

            videoPlayer.Stop();
            videoPlayer.targetCameraAlpha = 0;
            jumpscareUI?.SetActive(false);
        }

        Time.timeScale = 1;
        isJumpscaring = false;

        bossController?.OnJumpscareEnded();
    }
}
