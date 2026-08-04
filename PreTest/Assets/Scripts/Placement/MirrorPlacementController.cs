using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MirrorPlacementController : MonoBehaviour
{
    public const int MaxMirrorCount = 100;

    [SerializeField] private GameManager _gameManager;
    [SerializeField] private MirrorGhost _ghost;
    [SerializeField] private MirrorPool _mirrorPool;
    [SerializeField] private LayerMask _floorLayerMask;

    private Camera _camera;
    private bool _isPlacing;
    private bool _hasValidTarget;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    public bool IsPlacing => _isPlacing;

    private void Awake()
    {
        _camera = Camera.main;
        _ghost.Hide();
    }

    private void OnEnable()
    {
        _gameManager.ModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        _gameManager.ModeChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(AppMode mode)
    {
        if (mode != AppMode.Edit && _isPlacing)
        {
            CancelPlacement();
        }
    }

    public void BeginPlacement()
    {
        if (_gameManager.Mode != AppMode.Edit)
        {
            return;
        }

        if (PlacedMirror.ActiveCount >= MaxMirrorCount)
        {
            return;
        }

        _isPlacing = true;
        _ghost.Show();
    }

    private void CancelPlacement()
    {
        _isPlacing = false;
        _ghost.Hide();
    }

    private void Update()
    {
        if (!_isPlacing)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelPlacement();
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame && !InputFocusGuard.IsInputFieldFocused())
        {
            CancelPlacement();
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        UpdateHoveredTarget();

        if (_hasValidTarget && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceMirror();
        }
    }

    private void UpdateHoveredTarget()
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _floorLayerMask))
        {
            _hasValidTarget = false;
            _ghost.Hide();
            return;
        }

        // 격자 스냅 없이, Floor 레이어의 어떤 콜라이더든 표면 법선에 맞춰 자유 도킹.
        _hasValidTarget = true;
        _targetPosition = hit.point;
        _targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        _ghost.Show();
        _ghost.SetState(_targetPosition, _targetRotation, _hasValidTarget);
    }

    private void PlaceMirror()
    {
        _mirrorPool.Get(_targetPosition, _targetRotation);

        _isPlacing = false;
        _ghost.Hide();
    }
}
