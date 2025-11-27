using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class ImportantItemsCutscene : MonoBehaviour
{
    [Header("Cutscene Trigger")]
    public int requiredImportantItems = 5;
    public string diaryFlagKey = "DiaryImportantUnlocked";

    [Header("Cutscene Setup")]
    public VideoPlayer videoPlayer;
    public GameObject fullscreenUI;
    public MonoBehaviour playerMovementScript;
    public GameObject crosshair;

    [Header("Video Asset")]
    public VideoClip cutsceneClip;

    [Header("Optional Signals")]
    public GameObject[] objectsToDisableDuringCutscene;

    [Header("Post Cutscene Monologue")]
    [TextArea(2, 4)] public string postCutsceneMonologue = "Cái gì thế này?! Ánh sáng này... phát ra từ những món đồ kia? Tiếng khóc của con ma... đã tắt rồi. Chúng đang bảo vệ mình sao?";
    public float postCutsceneMonologueDuration = 5f;
    public bool postMonologueAddsToLog = true;

    private bool cutscenePlayed;

    private void OnEnable()
    {
        ImportantItemManager.OnAllImportantItemsCollected += HandleAllItemsCollected;
    }

    private void OnDisable()
    {
        ImportantItemManager.OnAllImportantItemsCollected -= HandleAllItemsCollected;
    }

    private void HandleAllItemsCollected()
    {
        if (cutscenePlayed) return;
        if (!string.IsNullOrEmpty(diaryFlagKey) && !StoryFlagManager.HasFlag(diaryFlagKey)) return;
        if (GlobalInventory.GetImportantItems().Count < requiredImportantItems) return;
        if (cutsceneClip == null || videoPlayer == null || fullscreenUI == null)
        {
            Debug.LogWarning("ImportantItemsCutscene: Missing video setup.");
            return;
        }

        StartCoroutine(PlayCutsceneRoutine());
    }

    private IEnumerator PlayCutsceneRoutine()
    {
        cutscenePlayed = true;

        foreach (var obj in objectsToDisableDuringCutscene)
        {
            if (obj != null) obj.SetActive(false);
        }

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (crosshair != null) crosshair.SetActive(false);

        fullscreenUI.SetActive(true);
        videoPlayer.clip = cutsceneClip;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        videoPlayer.Stop();
        fullscreenUI.SetActive(false);

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (crosshair != null) crosshair.SetActive(true);

        foreach (var obj in objectsToDisableDuringCutscene)
        {
            if (obj != null) obj.SetActive(true);
        }

        if (!string.IsNullOrWhiteSpace(postCutsceneMonologue))
        {
            float duration = postCutsceneMonologueDuration > 0f ? postCutsceneMonologueDuration : 5f;
            MonologueManager.PlayMonologue(postCutsceneMonologue, duration, postMonologueAddsToLog, true);
        }

        StoryFlagManager.SetFlag("ImportantItemsCutscenePlayed");
    }
}
