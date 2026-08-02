using UnityEngine;

public class LaserReceiver : MonoBehaviour, ILaserHitReceiver
{
    private static readonly int s_BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Color _hitColor = Color.cyan;
    [SerializeField] private Renderer _renderer;

    private MaterialPropertyBlock _propertyBlock;
    private Color _originalColor;
    private bool _isHitThisFrame;
    private bool _isActivated;

    private void Reset()
    {
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
        _originalColor = _renderer.sharedMaterial.GetColor(s_BaseColorId);
    }

    public void OnLaserHit()
    {
        _isHitThisFrame = true;

        if (_isActivated)
        {
            return;
        }

        _isActivated = true;
        ApplyColor(_hitColor);
    }

    private void LateUpdate()
    {
        if (_isHitThisFrame)
        {
            _isHitThisFrame = false;
            return;
        }

        if (!_isActivated)
        {
            return;
        }

        _isActivated = false;
        ApplyColor(_originalColor);
    }

    private void ApplyColor(Color color)
    {
        _propertyBlock.SetColor(s_BaseColorId, color);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
