using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Arrou : MonoBehaviour
{
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private Vector3Int targetTilePosition;
    [SerializeField] private Camera cam;
    [SerializeField] private float screenBorder = 0.9f;

    [Header("размер")]
    public float fixedSize = 1.0f;

    private SpriteRenderer spriteRenderer;
    private float originalOrthographicSize;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        originalOrthographicSize = cam.orthographicSize;
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
            float scale = fixedSize * (cam.orthographicSize / originalOrthographicSize);
            transform.localScale = new Vector3(scale, scale, 1f);
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