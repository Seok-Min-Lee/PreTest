using DG.Tweening;
using TMPro;
using UnityEngine;

public class MirrorCountDisplay : MonoBehaviour
{
    [SerializeField] private MirrorPlacementController _placementController;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private Color _maxReachedFlashColor = Color.red;
    [SerializeField] private float _flashDuration = 0.1f;

    private Color _normalColor;
    private Tween _flashTween;

    private void Awake()
    {
        _normalColor = _countText.color;
    }

    private void OnEnable()
    {
        PlacedMirror.ActiveCountChanged += HandleActiveCountChanged;
        _placementController.MaxMirrorCountReached += HandleMaxMirrorCountReached;
        Refresh();
    }

    private void OnDisable()
    {
        PlacedMirror.ActiveCountChanged -= HandleActiveCountChanged;
        _placementController.MaxMirrorCountReached -= HandleMaxMirrorCountReached;
        _flashTween?.Kill();
    }

    private void HandleActiveCountChanged(int activeCount)
    {
        Refresh();
    }

    private void HandleMaxMirrorCountReached()
    {
        _flashTween?.Kill();
        _countText.color = _normalColor;

        _flashTween = DOTween.To(() => _countText.color, x => _countText.color = x, _maxReachedFlashColor, _flashDuration)
            .SetLoops(4, LoopType.Yoyo);
    }

    private void Refresh()
    {
        _countText.text = $"{PlacedMirror.ActiveCount} / {GameConfig.Instance.MaxMirrorCount}";
    }
}
