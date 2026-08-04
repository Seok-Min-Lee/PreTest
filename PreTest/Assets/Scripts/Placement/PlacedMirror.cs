using System;
using UnityEngine;

public class PlacedMirror : MonoBehaviour
{
    public static int ActiveCount { get; private set; }
    public static event Action<int> ActiveCountChanged;

    public string DisplayName { get; set; }
    public string Description { get; set; }

    // 오브젝트 풀링 방식으로 동작하기 때문에 Awake/OnDestroy 대신 OnEnable/OnDisable로 카운트한다.
    private void OnEnable()
    {
        // 풀에서 재사용될 때 이전 배치의 값이 남아있지 않도록 매번 초기화.
        DisplayName = string.Empty;
        Description = string.Empty;

        ActiveCount++;
        ActiveCountChanged?.Invoke(ActiveCount);
    }

    private void OnDisable()
    {
        ActiveCount--;
        ActiveCountChanged?.Invoke(ActiveCount);
    }
}
