using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MirrorSelectionController : MonoBehaviour
{
    [SerializeField] private MirrorPlacementController _placementController;
    [SerializeField] private MirrorPool _mirrorPool;
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private LayerMask _mirrorLayerMask;
    [SerializeField] private LayerMask _gizmoHandleLayerMask;

    private Camera _camera;

    public PlacedMirror SelectedMirror { get; private set; }
    public event Action<PlacedMirror> SelectionChanged;
    public event Action<PlacedMirror> MirrorClicked;

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

        HandleDeleteShortcut();

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
            Select(null);
            return;
        }

        bool hitGizmoHandle = ((1 << hit.collider.gameObject.layer) & _gizmoHandleLayerMask.value) != 0;

        if (hitGizmoHandle)
        {
            // 핸들 클릭은 MirrorGizmo가 자체적으로 드래그 시작으로 처리 — 여기서 선택 해제하면 안 됨.
            return;
        }

        PlacedMirror mirror = hit.collider.GetComponent<PlacedMirror>();

        if (mirror != null)
        {
            MirrorClicked?.Invoke(mirror);
        }

        Select(mirror);
    }

    private void Select(PlacedMirror mirror)
    {
        if (SelectedMirror == mirror)
        {
            return;
        }

        SelectedMirror = mirror;
        SelectionChanged?.Invoke(mirror);
    }

    private void HandleDeleteShortcut()
    {
        if (SelectedMirror == null)
        {
            return;
        }

        if (InputFocusGuard.IsInputFieldFocused())
        {
            return;
        }

        if (!Keyboard.current.deleteKey.wasPressedThisFrame)
        {
            return;
        }

        DeleteSelected();
    }

    public void OnClickDelete()
    {
        DeleteSelected();
    }

    private void DeleteSelected()
    {
        if (_gameManager.Mode != AppMode.Edit)
        {
            return;
        }

        if (SelectedMirror == null)
        {
            return;
        }

        PlacedMirror mirror = SelectedMirror;
        Select(null);
        _mirrorPool.Release(mirror);
    }

    public void OnClickClear()
    {
        if (_gameManager.Mode != AppMode.Edit)
        {
            return;
        }

        Select(null);
        _mirrorPool.ReleaseAll();
        Debug.Log("[Clear] 모든 거울을 제거했습니다.");
    }
}
