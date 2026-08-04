using UnityEngine;

public class EditModeOnly : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    // 이 컴포넌트가 자기 자신이 붙은 오브젝트를 SetActive(false)로 끄기 때문에,
    // OnEnable/OnDisable로 구독하면 한 번 꺼진 뒤에는 다시 켜질 신호를 받을 수 없다.
    // 순수 C# event는 비활성 오브젝트에도 정상 호출되므로 Awake/OnDestroy로 구독한다.
    private void Awake()
    {
        _gameManager.ModeChanged += HandleModeChanged;
        ApplyMode(_gameManager.Mode);
    }

    private void OnDestroy()
    {
        _gameManager.ModeChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(AppMode mode)
    {
        ApplyMode(mode);
    }

    private void ApplyMode(AppMode mode)
    {
        gameObject.SetActive(mode == AppMode.Edit);
    }
}
