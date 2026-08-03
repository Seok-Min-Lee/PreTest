using System.Collections.Generic;
using UnityEngine;

public class FloorGrid : MonoBehaviour
{
    private const int MaxMirrorCount = 100;

    [SerializeField] private Collider _floorCollider;
    [SerializeField] private float _cellSize = 2f;

    private readonly Dictionary<Vector2Int, PlacedMirror> _occupied = new Dictionary<Vector2Int, PlacedMirror>();
    private Bounds _bounds;

    public int OccupiedCount => _occupied.Count;
    public bool IsFull => _occupied.Count >= MaxMirrorCount;
    public Collider FloorCollider => _floorCollider;

    private void Reset()
    {
        _floorCollider = GetComponent<Collider>();
    }

    private void Awake()
    {
        _bounds = _floorCollider.bounds;
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - _bounds.min.x) / _cellSize);
        int z = Mathf.FloorToInt((worldPosition.z - _bounds.min.z) / _cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        float x = _bounds.min.x + (cell.x + 0.5f) * _cellSize;
        float z = _bounds.min.z + (cell.y + 0.5f) * _cellSize;
        return new Vector3(x, _bounds.max.y, z);
    }

    public bool IsInBounds(Vector2Int cell)
    {
        int columnCount = Mathf.FloorToInt(_bounds.size.x / _cellSize);
        int rowCount = Mathf.FloorToInt(_bounds.size.z / _cellSize);

        if (cell.x < 0 || cell.x >= columnCount)
        {
            return false;
        }

        if (cell.y < 0 || cell.y >= rowCount)
        {
            return false;
        }

        return true;
    }

    public bool IsOccupied(Vector2Int cell)
    {
        return _occupied.ContainsKey(cell);
    }

    public void Occupy(Vector2Int cell, PlacedMirror mirror)
    {
        _occupied[cell] = mirror;
    }

    public void Free(Vector2Int cell)
    {
        _occupied.Remove(cell);
    }
}
