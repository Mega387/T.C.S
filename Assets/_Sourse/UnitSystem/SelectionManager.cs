using UnityEngine;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private LayerMask unitLayer;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isSelecting = false;
    private List<Unit> selectedUnits = new List<Unit>();
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isSelecting = true;
            startPos = cam.ScreenToWorldPoint(Input.mousePosition);
            endPos = startPos;
        }

        if (isSelecting && Input.GetKey(KeyCode.E))
        {
            endPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetKeyUp(KeyCode.E) && isSelecting)
        {
            isSelecting = false;

            foreach (Unit unit in selectedUnits)
            {
                if (unit != null) unit.isSelected = false;
            }
            selectedUnits.Clear();

            if (Vector2.Distance(startPos, endPos) > 0.05f)
            {
                Vector2 min = new Vector2(
                    Mathf.Min(startPos.x, endPos.x),
                    Mathf.Min(startPos.y, endPos.y)
                );
                Vector2 max = new Vector2(
                    Mathf.Max(startPos.x, endPos.x),
                    Mathf.Max(startPos.y, endPos.y)
                );

                Collider2D[] colliders = Physics2D.OverlapAreaAll(min, max, unitLayer);

                foreach (Collider2D col in colliders)
                {
                    Unit unit = col.GetComponent<Unit>();
                    if (unit != null && unit.tag == "UnitPlayer")
                    {
                        unit.isSelected = true;
                        selectedUnits.Add(unit);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            Vector2 target = cam.ScreenToWorldPoint(Input.mousePosition);
            foreach (Unit unit in selectedUnits)
            {
                if (unit != null) unit.MoveTo(target);
            }
        }
    }

    private void OnGUI()
    {
        if (isSelecting && Input.GetKey(KeyCode.E))
        {
            Vector2 screenStart = cam.WorldToScreenPoint(startPos);
            Vector2 screenEnd = cam.WorldToScreenPoint(endPos);

            screenStart.y = Screen.height - screenStart.y;
            screenEnd.y = Screen.height - screenEnd.y;

            Rect rect = new Rect(
                Mathf.Min(screenStart.x, screenEnd.x),
                Mathf.Min(screenStart.y, screenEnd.y),

                Mathf.Abs(screenStart.x - screenEnd.x),
                Mathf.Abs(screenStart.y - screenEnd.y)
            );

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0, 0.7f, 0.3f, 0.26f));
            texture.Apply();
            GUI.DrawTexture(rect, texture);
        }
    }
}