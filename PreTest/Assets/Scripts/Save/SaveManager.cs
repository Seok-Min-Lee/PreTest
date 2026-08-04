using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "mirrors.json";

    [SerializeField] private MirrorPool _mirrorPool;

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public void OnClickSave()
    {
        MirrorSaveDataList saveData = new MirrorSaveDataList();

        foreach (PlacedMirror mirror in _mirrorPool.ActiveMirrors)
        {
            saveData.Mirrors.Add(new MirrorSaveData
            {
                Position = mirror.transform.position,
                Rotation = mirror.transform.rotation,
                DisplayName = mirror.DisplayName,
                Description = mirror.Description
            });
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(saveData, true));
        Debug.Log($"[Save] 거울 {saveData.Mirrors.Count}개 저장 완료 -> {SavePath}");
    }
}
