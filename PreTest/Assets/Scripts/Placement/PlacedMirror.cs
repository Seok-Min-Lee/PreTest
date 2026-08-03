using UnityEngine;

public class PlacedMirror : MonoBehaviour
{
    private static readonly Vector2Int InvalidCell = new Vector2Int(int.MinValue, int.MinValue);

    private FloorGrid _floorGrid;

    public Vector2Int Cell { get; private set; } = InvalidCell;

    public void Initialize(FloorGrid floorGrid)
    {
        _floorGrid = floorGrid;
    }

    public void SetCell(Vector2Int cell)
    {
        ClearCell();
        Cell = cell;
        _floorGrid.Occupy(cell, this);
    }

    public void ClearCell()
    {
        if (Cell == InvalidCell)
        {
            return;
        }

        _floorGrid.Free(Cell);
        Cell = InvalidCell;
    }

    private void OnDestroy()
    {
        ClearCell();
    }
}
