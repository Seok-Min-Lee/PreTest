using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ModeIndicator _modeIndicator;

    private AppMode _mode = AppMode.Play;

    public AppMode Mode => _mode;
    public event Action<AppMode> ModeChanged;

    private void Start()
    {
        _modeIndicator.SetMode(_mode);
    }

    public void OnClickEditButton()
    {
        SetMode(AppMode.Edit);
    }

    public void OnClickPlayButton()
    {
        SetMode(AppMode.Play);
    }

    private void SetMode(AppMode mode)
    {
        if (_mode == mode)
        {
            return;
        }

        _mode = mode;
        _modeIndicator.SetMode(_mode);
        ModeChanged?.Invoke(_mode);
    }
}
