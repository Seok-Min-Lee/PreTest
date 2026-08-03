using UnityEngine;

public class PlacedMirror : MonoBehaviour
{
    public static int ActiveCount { get; private set; }

    // 오브젝트 풀링 방식으로 동작하기 때문에 Awake/OnDestroy 대신 OnEnable/OnDisable로 카운트한다.
    private void OnEnable()
    {
        ActiveCount++;
    }

    private void OnDisable()
    {
        ActiveCount--;
    }
}
