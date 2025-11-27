using System;
using UnityEngine;

public class DiaryImportantItemUnlocker : MonoBehaviour
{
    [Tooltip("Exact GameObject name of the diary note that must be read")]
    public string diaryNoteName = "Diary";

    [Tooltip("Important-item triggers or objects to toggle once the diary is read")]
    public GameObject[] objectsToEnable;

    public bool deactivateOnStart = true;
    public bool destroyAfterUnlock = true;

    private bool unlocked;

    private void Awake()
    {
        if (deactivateOnStart)
        {
            SetObjectsActive(false);
        }
    }

    private void OnEnable()
    {
        ReadNotebook.OnNoteOpened += HandleNoteOpened;
    }

    private void OnDisable()
    {
        ReadNotebook.OnNoteOpened -= HandleNoteOpened;
    }

    private void HandleNoteOpened(string openedNoteName)
    {
        if (unlocked) return;
        if (string.IsNullOrWhiteSpace(diaryNoteName)) return;
        if (!string.Equals(openedNoteName, diaryNoteName, StringComparison.OrdinalIgnoreCase)) return;

        unlocked = true;
        StoryFlagManager.SetFlag("DiaryImportantUnlocked");
        SetObjectsActive(true);

        if (destroyAfterUnlock)
        {
            Destroy(this);
        }
    }

    private void SetObjectsActive(bool state)
    {
        if (objectsToEnable == null) return;

        foreach (var obj in objectsToEnable)
        {
            if (obj == null) continue;
            obj.SetActive(state);
        }
    }
}
