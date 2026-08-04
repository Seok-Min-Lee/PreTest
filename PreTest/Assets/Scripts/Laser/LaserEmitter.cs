using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    private static readonly int s_EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private const float MaxRayDistance = 100f;
    private const float SelfCollisionOffset = 0.001f;
    private const float FrontFaceDotThreshold = 0.9f;
    private const int GizmoHandleLayer = 10;

    [SerializeField] private Transform _muzzle;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField, ColorUsage(true, true)] private Color _defaultColor = new Color(2f, 0.2f, 0.2f, 1f);
    [SerializeField, ColorUsage(true, true)] private Color _successColor = new Color(0f, 2f, 0f, 1f);
    [SerializeField, ColorUsage(true, true)] private Color _failureColor = new Color(0.15f, 0f, 0f, 1f);

    // GizmoHandle 레이어(Mirror 조작용 기즈모)는 퍼즐 판정과 무관하므로 레이저 충돌에서 제외.
    [SerializeField] private LayerMask _hittableLayers = ~(1 << GizmoHandleLayer);

    private int _maxReflectionCount;
    private Vector3[] _linePositions;
    private int _linePositionCount;
    private LaserResult _resultThisFrame;
    private MaterialPropertyBlock _propertyBlock;

    private void Reset()
    {
        _muzzle = transform;
        _lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    private void Awake()
    {
        _maxReflectionCount = GameConfig.Instance.MaxReflectionCount;
        _linePositions = new Vector3[_maxReflectionCount + 2];
        _propertyBlock = new MaterialPropertyBlock();

        if (_lineRenderer == null)
        {
            return;
        }

        _lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        SimulateLaser();
        DrawLine();
    }

    private void SimulateLaser()
    {
        _linePositionCount = 0;

        Vector3 direction = _muzzle.forward;
        Vector3 origin = _muzzle.position + direction * SelfCollisionOffset;
        AddLinePosition(_muzzle.position);

        for (int reflectionCount = 0; reflectionCount <= _maxReflectionCount; reflectionCount++)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, MaxRayDistance, _hittableLayers))
            {
                AddLinePosition(origin + direction * MaxRayDistance);
                _resultThisFrame = reflectionCount >= _maxReflectionCount ? LaserResult.Failure : LaserResult.Default;
                return;
            }

            AddLinePosition(hit.point);

            ILaserReflector reflector = hit.collider.GetComponent<ILaserReflector>();
            bool isFrontFaceHit = reflector != null
                && Vector3.Dot(hit.normal, reflector.ReflectiveNormal) > FrontFaceDotThreshold;
            bool canReflect = isFrontFaceHit && reflectionCount < _maxReflectionCount;

            if (canReflect)
            {
                direction = Vector3.Reflect(direction, hit.normal);
                origin = hit.point + direction * SelfCollisionOffset;
                continue;
            }

            ILaserHitReceiver receiver = hit.collider.GetComponent<ILaserHitReceiver>();

            if (receiver != null)
            {
                receiver.OnLaserHit();
                _resultThisFrame = LaserResult.Success;
            }
            else
            {
                _resultThisFrame = reflectionCount >= _maxReflectionCount ? LaserResult.Failure : LaserResult.Default;
            }

            return;
        }
    }

    private void AddLinePosition(Vector3 position)
    {
        _linePositions[_linePositionCount] = position;
        _linePositionCount++;
    }

    private void DrawLine()
    {
        if (_lineRenderer == null)
        {
            return;
        }

        _lineRenderer.positionCount = _linePositionCount;
        _lineRenderer.SetPositions(_linePositions);

        ApplyResultColor();
    }

    private void ApplyResultColor()
    {
        Color color = _defaultColor;

        if (_resultThisFrame == LaserResult.Success)
        {
            color = _successColor;
        }
        else if (_resultThisFrame == LaserResult.Failure)
        {
            color = _failureColor;
        }

        _propertyBlock.SetColor(s_EmissionColorId, color);
        _lineRenderer.SetPropertyBlock(_propertyBlock);
    }
}
