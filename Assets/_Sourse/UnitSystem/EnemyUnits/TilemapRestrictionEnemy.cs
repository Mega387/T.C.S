using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TilemapRestrictionEnemy : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap1;
    [SerializeField] private List<TileBase> restrictedTiles1 = new List<TileBase>();

    [SerializeField] private Tilemap tilemap2;
    [SerializeField] private List<TileBase> restrictedTiles2 = new List<TileBase>();

    [SerializeField] private Tilemap tilemap3;
    [SerializeField] private List<TileBase> restrictedTiles3 = new List<TileBase>();

    [SerializeField] private Tilemap buildingsTilemap;
    [SerializeField] private bool blockBuildingsForEnemies = true;

    [Header("Обход препятствий")]
    [SerializeField] private float avoidanceRadius = 1f;
    [SerializeField] private int avoidanceRays = 8;

    private Dictionary<Tilemap, HashSet<TileBase>> restrictedTileMap = new Dictionary<Tilemap, HashSet<TileBase>>();
    private Vector3Int lastCheckedCell = new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue);
    private bool lastWalkableResult = true;

    private void Start()
    {
        InitializeRestrictionMap();
    }

    private void InitializeRestrictionMap()
    {
        restrictedTileMap.Clear();

        if (tilemap1 != null && restrictedTiles1.Count > 0)
            restrictedTileMap[tilemap1] = new HashSet<TileBase>(restrictedTiles1);

        if (tilemap2 != null && restrictedTiles2.Count > 0)
            restrictedTileMap[tilemap2] = new HashSet<TileBase>(restrictedTiles2);

        if (tilemap3 != null && restrictedTiles3.Count > 0)
            restrictedTileMap[tilemap3] = new HashSet<TileBase>(restrictedTiles3);
    }

    public bool IsPositionWalkable(Vector2 worldPosition)
    {
        Vector3Int cellPosition = GetCellPosition(worldPosition);

        if (lastCheckedCell == cellPosition)
            return lastWalkableResult;

        lastCheckedCell = cellPosition;
        lastWalkableResult = CheckTileRestrictions(cellPosition);

        if (lastWalkableResult && blockBuildingsForEnemies && buildingsTilemap != null)
        {
            if (buildingsTilemap.HasTile(cellPosition))
                lastWalkableResult = false;
        }

        return lastWalkableResult;
    }

    public Vector2 GetAvoidanceDirection(Vector2 currentPosition, Vector2 targetDirection)
    {
        if (IsPositionWalkable(currentPosition + targetDirection))
            return targetDirection;

        float bestAngle = 0;
        float bestDistance = -1;

        for (int i = 0; i < avoidanceRays; i++)
        {
            float angle = (360f / avoidanceRays) * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 testDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Vector2 testPosition = currentPosition + testDirection * avoidanceRadius;

            if (IsPositionWalkable(testPosition))
            {
                float dot = Vector2.Dot(testDirection, targetDirection);
                if (dot > bestDistance)
                {
                    bestDistance = dot;
                    bestAngle = angle;
                }
            }
        }

        if (bestDistance > -0.5f)
        {
            float rad = bestAngle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        return Vector2.zero;
    }

    private Vector3Int GetCellPosition(Vector2 worldPosition)
    {
        if (tilemap1 != null)
            return tilemap1.WorldToCell(worldPosition);
        else if (tilemap2 != null)
            return tilemap2.WorldToCell(worldPosition);
        else if (tilemap3 != null)
            return tilemap3.WorldToCell(worldPosition);

        return Vector3Int.zero;
    }

    private bool CheckTileRestrictions(Vector3Int cellPosition)
    {
        foreach (var kvp in restrictedTileMap)
        {
            Tilemap tilemap = kvp.Key;
            HashSet<TileBase> restrictedTiles = kvp.Value;

            if (tilemap != null && tilemap.HasTile(cellPosition))
            {
                TileBase tile = tilemap.GetTile(cellPosition);
                if (restrictedTiles.Contains(tile))
                    return false;
            }
        }

        return true;
    }
    public bool IsTileRestricted(Vector2 worldPosition)
    {
        Vector3Int cellPosition = GetCellPosition(worldPosition);
        return !CheckTileRestrictions(cellPosition);
    }
    public Vector2 FindNearestWalkablePosition(Vector2 originalPosition)
    {
        if (IsPositionWalkable(originalPosition))
            return originalPosition;

        float searchRadius = 0.5f;
        float maxRadius = 5f;

        while (searchRadius <= maxRadius)
        {
            for (float angle = 0; angle < 360; angle += 45)
            {
                float rad = angle * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * searchRadius;
                Vector2 checkPosition = originalPosition + offset;

                if (IsPositionWalkable(checkPosition))
                    return checkPosition;
            }
            searchRadius += 0.5f;
        }

        return originalPosition;
    }
}