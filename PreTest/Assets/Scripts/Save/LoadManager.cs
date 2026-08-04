using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoSingleton<LoadManager>
{
    [SerializeField] private string _gameSceneName = "LaserTest";

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
        if (!File.Exists(SaveManager.SavePath))
        {
            Debug.Log("[Load] 저장 파일이 없어 빈 상태로 시작합니다.");
            return null;
        }

        try
        {
            MirrorSaveDataList saveData = JsonUtility.FromJson<MirrorSaveDataList>(File.ReadAllText(SaveManager.SavePath));
            Debug.Log($"[Load] 저장 파일 파싱 완료 -> {SaveManager.SavePath}");
            return saveData;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[Load] 저장 파일 파싱 실패, 빈 상태로 시작합니다: {exception.Message}");
            return null;
        }
    }
}
