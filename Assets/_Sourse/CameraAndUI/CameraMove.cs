using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraMove : MonoBehaviour
{
    [Header("Настройки перемещения камеры")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float keyboardPanSpeed = 10f;

    [Header("Настройки зума")]
    [SerializeField] private float zoomSpeed = 20f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;

    [Header("Автоматические границы")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private bool autoDetectBounds = true;
    [SerializeField] private float boundsUpdateDelay = 1f;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    private Bounds mapBounds;
    private bool boundsInitialized = false;
    private float gameStartTime;

    void Start()
    {
        cam = GetComponent<Camera>();
        gameStartTime = Time.time;

        mapBounds = new Bounds(Vector3.zero, new Vector3(100, 100, 0));

        if (targetTilemap != null)
        {
            transform.position = new Vector3(targetTilemap.transform.position.x, targetTilemap.transform.position.y, transform.position.z);
        }
    }

    void Update()
    {
        if (!boundsInitialized && autoDetectBounds && targetTilemap != null &&
            Time.time - gameStartTime >= boundsUpdateDelay)
        {
            CalculateTilemapBounds();
            boundsInitialized = true;
        }

        HandleDragMovement();
        HandleKeyboardMovement();
        HandleZoom();

        if (boundsInitialized)
        {
            ClampCameraPosition();
        }
    }

    void HandleDragMovement()
    {
        if (Input.GetMouseButtonDown(2))
        {
            isDragging = true;
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }
        if (isDragging)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragOrigin - currentPos;

            transform.position += difference;
        }
    }

    void HandleKeyboardMovement()
    {
        Vector3 position = transform.position;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            position.y += keyboardPanSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            position.y -= keyboardPanSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            position.x -= keyboardPanSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            position.x += keyboardPanSpeed * Time.deltaTime;

        transform.position = position;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Vector3 mouseWorldPosBeforeZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
            Vector3 mouseWorldPosAfterZoom = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += mouseWorldPosBeforeZoom - mouseWorldPosAfterZoom;

            if (boundsInitialized)
            {
                ClampCameraPosition();
            }
        }
    }

    void ClampCameraPosition()
    {
        if (cam == null) return;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float minX = mapBounds.min.x + horzExtent;
        float maxX = mapBounds.max.x - horzExtent;
        float minY = mapBounds.min.y + vertExtent;
        float maxY = mapBounds.max.y - vertExtent;

        if (maxX < minX) maxX = minX;
        if (maxY < minY) maxY = minY;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);

        transform.position = clampedPosition;
    }

    void CalculateTilemapBounds()
    {
        if (targetTilemap == null) return;

        targetTilemap.CompressBounds();
        mapBounds = targetTilemap.localBounds;

        if (mapBounds.size.x < 10f || mapBounds.size.y < 10f)
        {
            mapBounds = new Bounds(Vector3.zero, new Vector3(50, 50, 0));
        }

        Vector3 center = mapBounds.center;
        center.z = transform.position.z;
        transform.position = center;
        //границы
    }

    public void MoveToMapCenter()
    {
        if (targetTilemap != null && boundsInitialized)
        {
            Vector3 center = mapBounds.center;
            center.z = transform.position.z;
            transform.position = center;
        }
    }

    public void InitializeBounds()
    {
        if (targetTilemap != null)
        {
            CalculateTilemapBounds();
            boundsInitialized = true;
        }
    }
}