using UnityEngine;

public class GizmoHandle : MonoBehaviour
{
    [SerializeField] private GizmoHandleKind _kind;
    [SerializeField] private GizmoAxis _axis;

    public GizmoHandleKind Kind => _kind;
    public GizmoAxis Axis => _axis;
}
