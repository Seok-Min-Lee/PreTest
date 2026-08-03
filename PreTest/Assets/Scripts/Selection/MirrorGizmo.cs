using UnityEngine;
using UnityEngine.InputSystem;

public class MirrorGizmo : MonoBehaviour
{
    [SerializeField] private LayerMask _gizmoHandleLayerMask;

    private Camera _camera;
    private PlacedMirror _target;
    private GizmoHandleKind? _draggingHandle;
    private Plane _dragPlane;

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
        _dragPlane = new Plane(Vector3.up, _target.transform.position);

        if (_draggingHandle == GizmoHandleKind.Move)
        {
            // 자유 이동 사양: 드래그 시작 시 기존 그리드 점유를 해제해야 다른 배치가 그 셀을 다시 쓸 수 있음.
            _target.ClearCell();
        }
    }

    private void DragMove()
    {
        if (!RaycastDragPlane(out Vector3 point))
        {
            return;
        }

        Vector3 position = _target.transform.position;
        position.x = point.x;
        position.z = point.z;
        _target.transform.position = position;
        transform.position = position;
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

        // 바닥 수직축(Y) 기준 Yaw만 갱신 — X/Z 회전은 절대 건드리지 않아 항상 바닥에 수직으로 서 있음.
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        _target.transform.rotation = Quaternion.Euler(0f, angle, 0f);
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
