using UnityEngine;

public class GizmoModeToggle : MonoBehaviour
{
    [SerializeField] private MirrorGizmo _gizmo;
    [SerializeField] private GameObject _positionCover;
    [SerializeField] private GameObject _rotationCover;

    private void OnEnable()
    {
        _gizmo.ModeChanged += HandleModeChanged;

        ApplyMode(_gizmo.Mode);
    }

    private void OnDisable()
    {
        _gizmo.ModeChanged -= HandleModeChanged;
    }

    public void OnClickPosition()
    {
        _gizmo.SetMode(GizmoHandleKind.Move);
    }

    public void OnClickRotation()
    {
        _gizmo.SetMode(GizmoHandleKind.Rotate);
    }

    private void HandleModeChanged(GizmoHandleKind mode)
    {
        ApplyMode(mode);
    }

    private void ApplyMode(GizmoHandleKind mode)
    {
        _positionCover.SetActive(mode == GizmoHandleKind.Move);
        _rotationCover.SetActive(mode == GizmoHandleKind.Rotate);
    }
}
