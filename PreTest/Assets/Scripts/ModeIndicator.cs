using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModeIndicator : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Color _editColor = new Color(0f, 0.8980392f, 1f, 1f);
    [SerializeField] private Color _playColor = new Color(0f, 1f, 0.4f, 1f);

    public void SetMode(AppMode mode)
    {
        if (mode == AppMode.Edit)
        {
            ApplyColor(_editColor);
            _text.text = "MODE: EDIT";
            return;
        }

        ApplyColor(_playColor);
        _text.text = "MODE: PLAY";
    }

    private void ApplyColor(Color color)
    {
        _background.color = color;
        _icon.color = color;
        _text.color = color;
    }
}
