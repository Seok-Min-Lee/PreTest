using System;
using UnityEngine;

public class PlacedMirror : MonoBehaviour
{
    private const string DefaultDisplayName = "Mirror";

    public static int ActiveCount { get; private set; }
    public static event Action<int> ActiveCountChanged;

    // 삭제 후 재배치해도 번호가 겹치지 않도록 감소 없이 계속 증가만 한다.
    private static int s_NextDisplayNumber = 1;

    [SerializeField] private string _displayName = string.Empty;
    [SerializeField] private string _description = string.Empty;

    public string DisplayName
    {
        get => _displayName;
        set
        {
            _displayName = value;
            gameObject.name = string.IsNullOrEmpty(value) ? DefaultDisplayName : value;
        }
    }

    public string Description
    {
        get => _description;
        set => _description = value;
    }

    // 오브젝트 풀링 방식으로 동작하기 때문에 Awake/OnDestroy 대신 OnEnable/OnDisable로 카운트한다.
    private void OnEnable()
    {
        // 풀에서 재사용될 때 이전 배치의 값이 남아있지 않도록 매번 초기화.
        DisplayName = $"{DefaultDisplayName} {s_NextDisplayNumber}";
        s_NextDisplayNumber++;
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
