using UnityEngine;
using UnityEngine.Tilemaps;

public class TestTileClick : MonoBehaviour
{
    public Tilemap testTilemap;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedCell = testTilemap.WorldToCell(mouseWorldPos);
            TileBase clickedTile = testTilemap.GetTile(clickedCell);

            if (clickedTile != null)
            {
                Debug.Log("Клик по тайлу" +clickedTile.name + "позиция " +clickedCell);
            }
            else
            {
                Debug.Log("Пусто" +clickedCell);

            }
        }
    }
}