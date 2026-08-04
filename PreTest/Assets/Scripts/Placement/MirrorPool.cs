using System.Collections.Generic;
using UnityEngine;

public class MirrorPool : MonoBehaviour
{
    [SerializeField] private PlacedMirror _mirrorPrefab;

    private readonly List<PlacedMirror> _activeMirrors = new List<PlacedMirror>();
    private readonly Queue<PlacedMirror> _inactiveMirrors = new Queue<PlacedMirror>();

    public IReadOnlyList<PlacedMirror> ActiveMirrors => _activeMirrors;

    public PlacedMirror Get(Vector3 position, Quaternion rotation)
    {
        PlacedMirror mirror;

        if (_inactiveMirrors.Count > 0)
        {
            mirror = _inactiveMirrors.Dequeue();
        }
        else
        {
            mirror = Instantiate(_mirrorPrefab, transform);
        }

        mirror.transform.SetPositionAndRotation(position, rotation);
        mirror.gameObject.SetActive(true);
        _activeMirrors.Add(mirror);
        return mirror;
    }

    public void Release(PlacedMirror mirror)
    {
        mirror.gameObject.SetActive(false);
        _activeMirrors.Remove(mirror);
        _inactiveMirrors.Enqueue(mirror);
    }

    public void ReleaseAll()
    {
        List<PlacedMirror> mirrorsToRelease = new List<PlacedMirror>(_activeMirrors);

        foreach (PlacedMirror mirror in mirrorsToRelease)
        {
            Release(mirror);
        }
    }
}
