using UnityEngine;
using UnityEngine.UI;

public class ModeButtonGroup : MonoBehaviour
{
    [SerializeField] private Button _editButton;
    [SerializeField] private Button _playButton;

    private void OnEnable()
    {
        GameManager.ModeChanged += HandleModeChanged;
        ApplyMode(GameManager.Mode);
    }

    private void OnDisable()
    {
        GameManager.ModeChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(AppMode mode)
    {
        ApplyMode(mode);
    }

    private void ApplyMode(AppMode mode)
    {
        _editButton.gameObject.SetActive(mode != AppMode.Edit);
        _playButton.gameObject.SetActive(mode != AppMode.Play);
    }
}
