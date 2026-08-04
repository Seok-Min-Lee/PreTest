using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoSingleton<LoadManager>
{
    [SerializeField] private string _gameSceneName = "Scene";

    public MirrorSaveDataList LoadedData { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        LoadedData = TryLoad();
        SceneManager.LoadScene(_gameSceneName);
    }

    private static MirrorSaveDataList TryLoad()
    {
        bool hasSaveFile = File.Exists(SaveManager.SavePath);
        string loadPath = hasSaveFile ? SaveManager.SavePath : SaveManager.PresetPath;

        if (!File.Exists(loadPath))
        {
            Debug.Log("[Load] 저장 파일과 프리셋 파일이 모두 없어 빈 상태로 시작합니다.");
            return null;
        }

        try
        {
            MirrorSaveDataList saveData = JsonUtility.FromJson<MirrorSaveDataList>(File.ReadAllText(loadPath));
            Debug.Log(hasSaveFile
                ? $"[Load] 저장 파일 파싱 완료 -> {loadPath}"
                : $"[Load] 저장 파일이 없어 프리셋을 불러왔습니다 -> {loadPath}");
            return saveData;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[Load] 파일 파싱 실패, 빈 상태로 시작합니다: {exception.Message}");
            return null;
        }
    }
}
