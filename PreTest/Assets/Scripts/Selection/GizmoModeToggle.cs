using UnityEngine;

public class GizmoModeToggle : MonoBehaviour
{
    [SerializeField] private MirrorGizmo _gizmo;
    [SerializeField] private GameObject _positionCover;
    [SerializeField] private GameObject _rotationCover;

    private void OnEnable()
    {
        MirrorGizmo.ModeChanged += HandleModeChanged;

        ApplyMode(MirrorGizmo.Mode);
    }

    private void OnDisable()
    {
        MirrorGizmo.ModeChanged -= HandleModeChanged;
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
