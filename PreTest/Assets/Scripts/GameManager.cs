using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ModeIndicator _modeIndicator;
    [SerializeField] private MirrorPool _mirrorPool;

    private static AppMode s_Mode = AppMode.Play;

    public static AppMode Mode => s_Mode;
    public static event Action<AppMode> ModeChanged;

    private void Start()
    {
        _modeIndicator.SetMode(s_Mode);
        SpawnLoadedMirrors();
    }

    private void SpawnLoadedMirrors()
    {
        if (LoadManager.Instance == null)
        {
            return;
        }

        MirrorSaveDataList saveData = LoadManager.Instance.LoadedData;

        if (saveData == null || saveData.Mirrors == null)
        {
            return;
        }

        int spawnedCount = 0;

        foreach (MirrorSaveData data in saveData.Mirrors)
        {
            if (PlacedMirror.ActiveCount >= GameConfig.Instance.MaxMirrorCount)
            {
                break;
            }

            PlacedMirror mirror = _mirrorPool.Get(data.Position, data.Rotation);
            mirror.DisplayName = data.DisplayName;
            mirror.Description = data.Description;
            spawnedCount++;
        }

        Debug.Log($"[Load] 거울 {spawnedCount}개 복원 완료 (저장된 값 {saveData.Mirrors.Count}개 중)");
    }

    public void OnClickEditButton()
    {
        SetMode(AppMode.Edit);
    }

    public void OnClickPlayButton()
    {
        SetMode(AppMode.Play);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetMode(AppMode mode)
    {
        if (s_Mode == mode)
        {
            return;
        }

        s_Mode = mode;
        _modeIndicator.SetMode(s_Mode);
        ModeChanged?.Invoke(s_Mode);
    }
}
