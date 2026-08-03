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
    private bool _hasValidCell;
    private Vector2Int _hoveredCell;

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

        UpdateHoveredCell();

        if (_hasValidCell && Mouse.current.leftButton.wasPressedThisFrame)
        {
            PlaceMirror();
        }
    }

    private void UpdateHoveredCell()
    {
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _floorLayerMask))
        {
            _hasValidCell = false;
            _ghost.Hide();
            return;
        }

        _hoveredCell = _floorGrid.WorldToCell(hit.point);
        _hasValidCell = _floorGrid.IsInBounds(_hoveredCell) && !_floorGrid.IsOccupied(_hoveredCell);

        _ghost.Show();
        _ghost.SetState(_floorGrid.CellToWorldCenter(_hoveredCell), _hasValidCell);
    }

    private void PlaceMirror()
    {
        Vector3 spawnPosition = _floorGrid.CellToWorldCenter(_hoveredCell);
        GameObject instance = Instantiate(_mirrorPrefab, spawnPosition, Quaternion.identity);

        PlacedMirror placedMirror = instance.GetComponent<PlacedMirror>();
        placedMirror.Initialize(_floorGrid);
        placedMirror.SetCell(_hoveredCell);

        _isPlacing = false;
        _ghost.Hide();

        if (_floorGrid.IsFull)
        {
            _addMirrorButton.interactable = false;
        }
    }
}
