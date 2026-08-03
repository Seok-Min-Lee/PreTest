using UnityEngine;

public class GizmoHandle : MonoBehaviour
{
    [SerializeField] private GizmoHandleKind _kind;

    public GizmoHandleKind Kind => _kind;
}
