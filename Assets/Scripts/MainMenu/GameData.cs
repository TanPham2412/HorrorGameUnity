using UnityEngine;

[System.Serializable]
public class GameData
{
    public string sceneName;

    public Vector3 playerPosition;

    public GameData()
    {
        this.sceneName = "Scene_TruongHoc_Dem1";
        this.playerPosition = Vector3.zero; 
    }
}