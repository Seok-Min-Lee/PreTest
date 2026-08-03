using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorGizmo : MonoBehaviour
{
    private const int FloorLayer = 8;

    [SerializeField] private LayerMask _gizmoHandleLayerMask;
    [SerializeField] private LayerMask _floorLayerMask = 1 << FloorLayer;

    private Camera _camera;
    private PlacedMirror _target;
    private GizmoHandleKind? _draggingHandle;
    private Plane _dragPlane;
    private Vector3 _rotateAxis;
    private Vector3 _rotateStartDirection;
    private Quaternion _rotateStartRotation;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Attach(PlacedMirror target)
    {
        _target = target;
        transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
        gameObject.SetActive(true);
    }

    public void Detach()
    {
        _target = null;
        _draggingHandle = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        HandleDragInput();

        if (_draggingHandle == null)
        {
            transform.SetPositionAndRotation(_target.transform.position, _target.transform.rotation);
        }
    }

    private void HandleDragInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryBeginDrag();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _draggingHandle = null;
        }

        if (_draggingHandle == GizmoHandleKind.Move)
        {
            DragMove();
        }
        else if (_draggingHandle == GizmoHandleKind.Rotate)
        {
            DragRotate();
        }
    }

    private void TryBeginDrag()
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _gizmoHandleLayerMask))
        {
            return;
        }

        GizmoHandle handle = hit.collider.GetComponent<GizmoHandle>();

        if (handle == null)
        {
            return;
        }

        _draggingHandle = handle.Kind;

        if (_draggingHandle == GizmoHandleKind.Rotate)
        {
            BeginRotateDrag();
        }
    }

    private void BeginRotateDrag()
    {
        // 현재 서 있는 표면의 법선을 회전축으로 고정 — 경사면 위에서도 표면에 붙은 채로 돌도록.
        _rotateAxis = _target.transform.up;
        _rotateStartRotation = _target.transform.rotation;
        _dragPlane = new Plane(_rotateAxis, _target.transform.position);

        if (RaycastDragPlane(out Vector3 point))
        {
            _rotateStartDirection = (point - _target.transform.position).normalized;
        }
    }

    private void DragMove()
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _floorLayerMask))
        {
            return;
        }

        // 직전 up 벡터 -> 새 표면 법선으로의 회전 델타만 적용해 기존 트위스트(바라보는 방향)를 유지.
        Quaternion normalDelta = Quaternion.FromToRotation(_target.transform.up, hit.normal);
        _target.transform.SetPositionAndRotation(hit.point, normalDelta * _target.transform.rotation);
        transform.SetPositionAndRotation(_target.transform.position, _target.transform.rotation);
    }

    private void DragRotate()
    {
        if (!RaycastDragPlane(out Vector3 point))
        {
            return;
        }

        Vector3 direction = point - _target.transform.position;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float deltaAngle = Vector3.SignedAngle(_rotateStartDirection, direction.normalized, _rotateAxis);
        _target.transform.rotation = Quaternion.AngleAxis(deltaAngle, _rotateAxis) * _rotateStartRotation;
        transform.rotation = _target.transform.rotation;
    }

    private bool RaycastDragPlane(out Vector3 point)
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (_dragPlane.Raycast(ray, out float distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        point = Vector3.zero;
        return false;
    }
}
