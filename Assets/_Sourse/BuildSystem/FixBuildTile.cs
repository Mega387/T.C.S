using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FixBuildTile : MonoBehaviour
{
    [SerializeField] private Tilemap buildTilemap;
    [Header("-----------------")]
    [SerializeField] private TileBase wall0;
    [SerializeField] private TileBase wall90;
    [SerializeField] private TileBase wall360;
    [Header("-----------------")]
    [SerializeField] private TileBase wallDownRight;
    [SerializeField] private TileBase wallDownLeft;
    [SerializeField] private TileBase wallTopRight;
    [SerializeField] private TileBase wallTopLeft;
    [Header("-----------------")]
    [SerializeField] private TileBase wallTtop;
    [SerializeField] private TileBase wallTDown;
    [SerializeField] private TileBase wallTLeft;
    [SerializeField] private TileBase wallTRight;

    private HashSet<TileBase> finishedWalls;

    void Awake()
    {
        finishedWalls = new HashSet<TileBase>
        {
            wall0, wall90, wall360,
            wallDownRight, wallDownLeft,
            wallTopRight, wallTopLeft,
            wallTtop, wallTDown,
            wallTLeft, wallTRight
        };
    }

    public void shiftCount()
    {
        for (int pass = 0; pass < 3; pass++)
        {
            fixWall();
        }
    }

    void fixWall()
    {
        BoundsInt bounds = buildTilemap.cellBounds;
        List<Vector3Int> positions = new List<Vector3Int>();

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = buildTilemap.GetTile(pos);

                if (tile != null && finishedWalls.Contains(tile))
                {
                    positions.Add(pos);
                }
            }
        }

        foreach (Vector3Int pos in positions)
        {
            bool hasUp = HasFinishedWallAt(pos + Vector3Int.up);
            bool hasDown = HasFinishedWallAt(pos + Vector3Int.down);
            bool hasLeft = HasFinishedWallAt(pos + Vector3Int.left);
            bool hasRight = HasFinishedWallAt(pos + Vector3Int.right);

            TileBase newTile = GetCorrectWall(hasUp, hasDown, hasLeft, hasRight);

            if (newTile != null)
            {
                TileBase current = buildTilemap.GetTile(pos);
                if (current != newTile)
                {
                    buildTilemap.SetTile(pos, newTile);
                }
            }
        }
    }

    bool HasFinishedWallAt(Vector3Int pos)
    {
        TileBase tile = buildTilemap.GetTile(pos);
        return tile != null && finishedWalls.Contains(tile);
    }

    TileBase GetCorrectWall(bool up, bool down, bool left, bool right)
    {
        int count = (up ? 1 : 0) + (down ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);
        if (count == 4) return wall360;
        if (count == 3)
        {
            if (!up) return wallTDown;     // нет сверху - ветка вниз ┬
            if (!down) return wallTtop;    // нет снизу - ветка вверх ┴
            if (!left) return wallTRight;  // нет слева - ветка вправо ├
            if (!right) return wallTLeft;  // нет справа - ветка влево ┤
        }
        if (count == 2)
        {
            if (up && down) return wall90;
            if (left && right) return wall0;
            if (up && right) return wallTopRight;
            if (up && left) return wallTopLeft;
            if (down && right) return wallDownRight;
            if (down && left) return wallDownLeft;
        }
        if (count == 1)
        {
            if (up || down) return wall90;
            if (left || right) return wall0;
        }

        return null;
    }
}