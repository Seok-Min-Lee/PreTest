using UnityEngine;

public class PlacedMirror : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    private void Awake()
    {
        ActiveCount++;
    }

    private void OnDestroy()
    {
        ActiveCount--;
    }
}
