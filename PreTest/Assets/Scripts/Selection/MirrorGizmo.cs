using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorGizmo : MonoBehaviour
{
    [SerializeField] private LayerMask _gizmoHandleLayerMask;
    [SerializeField] private MirrorSelectionController _selectionController;

    private static GizmoHandleKind s_Mode = GizmoHandleKind.Move;

    private Camera _camera;
    private GizmoHandle[] _handles;
    private PlacedMirror _target;
    private GizmoHandleKind? _draggingHandle;
    private Plane _dragPlane;
    private Vector3 _dragAxis;
    private Vector3 _moveDragStartPoint;
    private Vector3 _moveDragStartPosition;
    private Vector3 _rotateStartDirection;
    private Quaternion _rotateStartRotation;

    public static GizmoHandleKind Mode => s_Mode;
    public static event Action<GizmoHandleKind> ModeChanged;

    private void Awake()
    {
        _camera = Camera.main;
        _handles = GetComponentsInChildren<GizmoHandle>(true);
        _selectionController.SelectionChanged += HandleSelectionChanged;
        GameManager.ModeChanged += HandleModeChanged;
        UpdateVisibility();
    }

    private void OnDestroy()
    {
        _selectionController.SelectionChanged -= HandleSelectionChanged;
        GameManager.ModeChanged -= HandleModeChanged;
    }

    private void HandleSelectionChanged(PlacedMirror mirror)
    {
        _target = mirror;
        _draggingHandle = null;

        if (_target != null)
        {
            transform.SetPositionAndRotation(_target.transform.position, _target.transform.rotation);
            ApplyMode();
        }

        UpdateVisibility();
    }

    private void HandleModeChanged(AppMode mode)
    {
        UpdateVisibility();
    }

    // Play 모드에서는 거울을 선택할 수는 있지만(Inspector 표시용) 기즈모는 뜨면 안 되므로,
    // "선택됨"과 "기즈모를 실제로 보여줌"을 분리해 여기서 최종 표시 여부를 판단한다.
    private void UpdateVisibility()
    {
        bool shouldShow = _target != null && GameManager.Mode == AppMode.Edit;
        gameObject.SetActive(shouldShow);
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        HandleModeInput();
        HandleDragInput();

        if (_draggingHandle == null)
        {
            transform.SetPositionAndRotation(_target.transform.position, _target.transform.rotation);
        }
    }

    private void HandleModeInput()
    {
        if (_draggingHandle != null)
        {
            return;
        }

        if (InputFocusGuard.IsInputFieldFocused())
        {
            return;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            SetMode(GizmoHandleKind.Move);
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            SetMode(GizmoHandleKind.Rotate);
        }
    }

    public void SetMode(GizmoHandleKind mode)
    {
        if (s_Mode == mode)
        {
            return;
        }

        s_Mode = mode;
        ApplyMode();
        ModeChanged?.Invoke(s_Mode);
    }

    private void ApplyMode()
    {
        foreach (GizmoHandle handle in _handles)
        {
            handle.gameObject.SetActive(handle.Kind == s_Mode);
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
        Vector3 axis = GetAxisDirection(handle.Axis);

        if (_draggingHandle == GizmoHandleKind.Move)
        {
            BeginMoveDrag(axis);
        }
        else if (_draggingHandle == GizmoHandleKind.Rotate)
        {
            BeginRotateDrag(axis);
        }
    }

    private Vector3 GetAxisDirection(GizmoAxis axis)
    {
        if (axis == GizmoAxis.X)
        {
            return _target.transform.right;
        }

        if (axis == GizmoAxis.Y)
        {
            return _target.transform.up;
        }

        return _target.transform.forward;
    }

    private void BeginMoveDrag(Vector3 axis)
    {
        _dragAxis = axis;
        _moveDragStartPosition = _target.transform.position;
        RaycastAxis(axis, _moveDragStartPosition, out _moveDragStartPoint);
    }

    private void BeginRotateDrag(Vector3 axis)
    {
        _dragAxis = axis;
        _rotateStartRotation = _target.transform.rotation;
        _dragPlane = new Plane(axis, _target.transform.position);

        if (RaycastDragPlane(out Vector3 point))
        {
            _rotateStartDirection = (point - _target.transform.position).normalized;
        }
    }

    private void DragMove()
    {
        if (!RaycastAxis(_dragAxis, _moveDragStartPosition, out Vector3 point))
        {
            return;
        }

        Vector3 delta = point - _moveDragStartPoint;
        _target.transform.position = _moveDragStartPosition + delta;
        transform.position = _target.transform.position;
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

        float deltaAngle = Vector3.SignedAngle(_rotateStartDirection, direction.normalized, _dragAxis);
        _target.transform.rotation = Quaternion.AngleAxis(deltaAngle, _dragAxis) * _rotateStartRotation;
        transform.rotation = _target.transform.rotation;
    }

    private bool RaycastAxis(Vector3 axis, Vector3 origin, out Vector3 pointOnAxis)
    {
        // 카메라 방향과 축을 함께 포함하는 평면에 레이캐스트한 뒤, 그 교차점을 축 위로 투영.
        Vector3 toCamera = _camera.transform.position - origin;
        Vector3 planeNormal = Vector3.Cross(axis, Vector3.Cross(toCamera, axis));

        if (planeNormal.sqrMagnitude < 0.0001f)
        {
            pointOnAxis = origin;
            return false;
        }

        Plane plane = new Plane(planeNormal.normalized, origin);
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!plane.Raycast(ray, out float distance))
        {
            pointOnAxis = origin;
            return false;
        }

        Vector3 hitPoint = ray.GetPoint(distance);
        pointOnAxis = origin + axis * Vector3.Dot(hitPoint - origin, axis);
        return true;
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
