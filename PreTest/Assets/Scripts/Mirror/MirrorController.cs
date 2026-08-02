using UnityEngine;

public class MirrorController : MonoBehaviour, ILaserReflector
{
    public Vector3 ReflectiveNormal => transform.forward;
}
