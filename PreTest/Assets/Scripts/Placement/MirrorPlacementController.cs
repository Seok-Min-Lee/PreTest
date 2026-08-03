using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MirrorPlacementController : MonoBehaviour
{
    [SerializeField] private FloorGrid _floorGrid;
    [SerializeField] private MirrorGhost _ghost;
    [SerializeField] private GameObject _mirrorPrefab;
    [SerializeField] private Button _addMirrorButton;
    [SerializeField] private LayerMask _floorLayerMask;

    private Camera _camera;
    private bool _isPlacing;
    private bool _hasValidTarget;
    private bool _isGridTarget;
    private Vector2Int _targetCell;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    public bool IsPlacing => _isPlacing;

    private void Awake()
    {
        _camera = Camera.main;
        _ghost.Hide();
    }

    public void BeginPlacement()
    {
        if (_floorGrid.IsFull)
        {
            return;
        }

        _isPlacing = true;
        _ghost.Show();
    }

    private void Update()
    {
        if (!_isPlacing)
        {
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

        _isGridTarget = hit.collider == _floorGrid.FloorCollider;

        if (_isGridTarget)
        {
            _targetCell = _floorGrid.WorldToCell(hit.point);
            _hasValidTarget = _floorGrid.IsInBounds(_targetCell) && !_floorGrid.IsOccupied(_targetCell);
            _targetPosition = _floorGrid.CellToWorldCenter(_targetCell);
            _targetRotation = Quaternion.identity;
        }
        else
        {
            // 그리드가 아닌 Floor 레이어 콜라이더(예: 경사면)는 격자 없이 표면 법선에 맞춰 자유 도킹.
            _hasValidTarget = true;
            _targetPosition = hit.point;
            _targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        _ghost.Show();
        _ghost.SetState(_targetPosition, _targetRotation, _hasValidTarget);
    }

    private void PlaceMirror()
    {
        GameObject instance = Instantiate(_mirrorPrefab, _targetPosition, _targetRotation);

        if (_isGridTarget)
        {
            PlacedMirror placedMirror = instance.GetComponent<PlacedMirror>();
            placedMirror.Initialize(_floorGrid);
            placedMirror.SetCell(_targetCell);
        }

        _isPlacing = false;
        _ghost.Hide();

        if (_floorGrid.IsFull)
        {
            _addMirrorButton.interactable = false;
        }
    }
}
