using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class ImportantItemsCutscene : MonoBehaviour
{
    public static event Action PostCutsceneMonologuesFinished;

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
    public float defaultPostCutsceneMonologueDuration = 5f;
    public bool postMonologueAddsToLog = true;
    public List<PostCutsceneMonologueLine> postCutsceneMonologueLines = new();

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

        if (postCutsceneMonologueLines != null && postCutsceneMonologueLines.Count > 0)
        {
            foreach (var line in postCutsceneMonologueLines)
            {
                if (line == null || string.IsNullOrWhiteSpace(line.text)) continue;
                float duration = line.duration > 0f ? line.duration : defaultPostCutsceneMonologueDuration;
                duration = duration > 0f ? duration : 5f;
                MonologueManager.PlayMonologue(line.text, duration, postMonologueAddsToLog, true);
            }
        }
        else
        {
            Debug.LogWarning("ImportantItemsCutscene: No post cutscene monologue lines configured.");
        }

        PostCutsceneMonologuesFinished?.Invoke();
        StoryFlagManager.SetFlag("ImportantItemsCutscenePlayed");
    }

    [System.Serializable]
    public class PostCutsceneMonologueLine
    {
        [TextArea(2, 4)] public string text;
        public float duration = 5f;
    }
}
