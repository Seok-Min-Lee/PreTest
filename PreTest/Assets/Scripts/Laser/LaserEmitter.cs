using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    private const int MaxReflectionCount = 10;
    private const float MaxRayDistance = 100f;
    private const float SelfCollisionOffset = 0.001f;
    private const float FrontFaceDotThreshold = 0.9f;
    private const int GizmoHandleLayer = 10;

    [SerializeField] private Transform _muzzle;
    [SerializeField] private LineRenderer _lineRenderer;

    // GizmoHandle 레이어(Mirror 조작용 기즈모)는 퍼즐 판정과 무관하므로 레이저 충돌에서 제외.
    [SerializeField] private LayerMask _hittableLayers = ~(1 << GizmoHandleLayer);

    private readonly Vector3[] _linePositions = new Vector3[MaxReflectionCount + 2];
    private int _linePositionCount;

    private void Reset()
    {
        _muzzle = transform;
        _lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    private void Awake()
    {
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

        for (int reflectionCount = 0; reflectionCount <= MaxReflectionCount; reflectionCount++)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, MaxRayDistance, _hittableLayers))
            {
                AddLinePosition(origin + direction * MaxRayDistance);
                return;
            }

            AddLinePosition(hit.point);

            ILaserReflector reflector = hit.collider.GetComponent<ILaserReflector>();
            bool isFrontFaceHit = reflector != null
                && Vector3.Dot(hit.normal, reflector.ReflectiveNormal) > FrontFaceDotThreshold;
            bool canReflect = isFrontFaceHit && reflectionCount < MaxReflectionCount;

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
    }
}
