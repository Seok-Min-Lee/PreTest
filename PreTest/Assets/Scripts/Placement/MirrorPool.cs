using System.Collections.Generic;
using UnityEngine;

public class MirrorPool : MonoBehaviour
{
    [SerializeField] private PlacedMirror _mirrorPrefab;

    private readonly Stack<PlacedMirror> _inactiveMirrors = new Stack<PlacedMirror>();

    public PlacedMirror Get(Vector3 position, Quaternion rotation)
    {
        PlacedMirror mirror;

        if (_inactiveMirrors.Count > 0)
        {
            mirror = _inactiveMirrors.Pop();
        }
        else
        {
            mirror = Instantiate(_mirrorPrefab, transform);
        }

        mirror.transform.SetPositionAndRotation(position, rotation);
        mirror.gameObject.SetActive(true);
        return mirror;
    }

    public void Release(PlacedMirror mirror)
    {
        mirror.gameObject.SetActive(false);
        _inactiveMirrors.Push(mirror);
    }
}
