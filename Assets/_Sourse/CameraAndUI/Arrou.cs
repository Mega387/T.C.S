using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Arrou : MonoBehaviour
{
    public Tilemap targetTilemap;
    public Vector3Int targetTilePosition;
    public Camera cam;
    public float screenBorder = 0.9f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    void Update()
    {
        if (targetTilemap == null) return;

        Vector3 worldPos = targetTilemap.CellToWorld(targetTilePosition) + new Vector3(0.5f, 0.5f, 0);
        Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

        bool onScreen = viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

        spriteRenderer.enabled = !onScreen;

        if (!onScreen)
        {
            UpdateArrowPosition(viewportPos);
        }
    }

    void UpdateArrowPosition(Vector3 viewportPos)
    {
        Vector3 screenCenter = new Vector3(0.5f, 0.5f, 0);
        Vector3 dir = (viewportPos - screenCenter).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
        float y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);

        Vector3 clampedViewportPos = new Vector3(x, y, viewportPos.z);
        Vector3 worldPos = cam.ViewportToWorldPoint(clampedViewportPos);
        transform.position = new Vector3(worldPos.x, worldPos.y, 0);
    }

    public void SetTarget(Tilemap tilemap, Vector3Int position)
    {
        targetTilemap = tilemap;
        targetTilePosition = position;
    }
}