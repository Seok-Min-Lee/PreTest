using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupAlphaLoop : MonoBehaviour
{
    [SerializeField] private float _minAlpha = 0.3f;
    [SerializeField] private float _maxAlpha = 1f;
    [SerializeField] private float _duration = 1f;

    private CanvasGroup _canvasGroup;
    private Tween _alphaTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = _minAlpha;

        _alphaTween = _canvasGroup
            .DOFade(_maxAlpha, _duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        _alphaTween?.Kill();
    }
}
