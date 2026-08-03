using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MirrorSelectionController : MonoBehaviour
{
    [SerializeField] private MirrorGizmo _gizmo;
    [SerializeField] private MirrorPlacementController _placementController;
    [SerializeField] private LayerMask _mirrorLayerMask;
    [SerializeField] private LayerMask _gizmoHandleLayerMask;

    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (_placementController.IsPlacing)
        {
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        int combinedMask = _mirrorLayerMask | _gizmoHandleLayerMask;
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, combinedMask))
        {
            _gizmo.Detach();
            return;
        }

        bool hitGizmoHandle = ((1 << hit.collider.gameObject.layer) & _gizmoHandleLayerMask.value) != 0;

        if (hitGizmoHandle)
        {
            // 핸들 클릭은 MirrorGizmo가 자체적으로 드래그 시작으로 처리 — 여기서 선택 해제하면 안 됨.
            return;
        }

        PlacedMirror mirror = hit.collider.GetComponent<PlacedMirror>();
        _gizmo.Attach(mirror);
    }
}
