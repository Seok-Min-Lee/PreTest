using UnityEngine;

public class MirrorGhost : MonoBehaviour
{
    private static readonly int s_BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.5f);

    private MaterialPropertyBlock _propertyBlock;

    private void Reset()
    {
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    public void SetState(Vector3 worldPosition, Quaternion worldRotation, bool isValid)
    {
        transform.SetPositionAndRotation(worldPosition, worldRotation);

        _propertyBlock.SetColor(s_BaseColorId, isValid ? _validColor : _invalidColor);
        _renderer.SetPropertyBlock(_propertyBlock);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
