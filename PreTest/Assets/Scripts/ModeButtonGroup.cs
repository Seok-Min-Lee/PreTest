using UnityEngine;
using UnityEngine.UI;

public class ModeButtonGroup : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Button _editButton;
    [SerializeField] private Button _playButton;

    private void OnEnable()
    {
        _gameManager.ModeChanged += HandleModeChanged;
        ApplyMode(_gameManager.Mode);
    }

    private void OnDisable()
    {
        _gameManager.ModeChanged -= HandleModeChanged;
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
