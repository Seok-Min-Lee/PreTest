using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LaserReceiver : MonoBehaviour, ILaserHitReceiver
{
    private static readonly int s_BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Color _hitColor = Color.cyan;
    [SerializeField] private Color _idleColor = Color.red;

    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _isHitThisFrame;
    private bool _isActivated;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        ApplyColor(_idleColor);
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
        ApplyColor(_idleColor);
    }

    private void ApplyColor(Color color)
    {
        _propertyBlock.SetColor(s_BaseColorId, color);
        _renderer.SetPropertyBlock(_propertyBlock);
    }
}
