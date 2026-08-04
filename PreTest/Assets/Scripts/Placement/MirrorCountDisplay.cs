using TMPro;
using UnityEngine;

public class MirrorCountDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countText;

    private void OnEnable()
    {
        PlacedMirror.ActiveCountChanged += HandleActiveCountChanged;
        Refresh();
    }

    private void OnDisable()
    {
        PlacedMirror.ActiveCountChanged -= HandleActiveCountChanged;
    }

    private void HandleActiveCountChanged(int activeCount)
    {
        Refresh();
    }

    private void Refresh()
    {
        _countText.text = $"{PlacedMirror.ActiveCount} / {MirrorPlacementController.MaxMirrorCount}";
    }
}
