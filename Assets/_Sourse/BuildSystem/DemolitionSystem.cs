using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DemolitionSystem : MonoBehaviour
{
    [Header("Тайлмапы")]
    [SerializeField] private Tilemap buildingsTilemap;
    [SerializeField] private Tilemap highlightTilemap;

    [Header("Тайлы подсветки")]
    [SerializeField] private TileBase mainHighlightTile;
    [SerializeField] private TileBase chainHighlightTile;

    [Header("Настройки")]
    [SerializeField] private KeyCode demolishKey = KeyCode.Mouse0;
    [SerializeField] private int maxChainDepth = 10;
    [SerializeField] private List<BuildingRule> buildingRules;

    [System.Serializable]
    public class BuildingRule
    {
        public string ruleName;
        public TileBase targetBuilding;
        public List<DestroyTarget> destroyWithIt;
        public bool useGlobalCondition = false;
        public GlobalCondition globalCondition;
    }

    [System.Serializable]
    public class DestroyTarget
    {
        public string targetName;
        public TileBase targetTile1;
        public TileBase targetTile2;
        public TileBase targetTile3;
        public TileBase targetTile4;
        public int searchRadius = 0;
        public bool requireCondition = false;
        public ConditionType conditionType;
        public TileBase conditionCheckBuilding1;
        public TileBase conditionCheckBuilding2;
        public TileBase conditionCheckBuilding3;
        public TileBase conditionCheckBuilding4;
        public int conditionRadius = 1;
        public int requiredCount = 1;
        public bool inverseCondition = false;

        public enum ConditionType
        {
            None,
            DestroyOnlyIfNoNearbyBuildings,
            DestroyOnlyIfNearbyBuildings,
            DestroyOnlyIfNearbyBuildingsCount
        }

        public List<TileBase> GetTargetTiles()
        {
            List<TileBase> tiles = new List<TileBase>();
            if (targetTile1 != null) tiles.Add(targetTile1);
            if (targetTile2 != null) tiles.Add(targetTile2);
            if (targetTile3 != null) tiles.Add(targetTile3);
            if (targetTile4 != null) tiles.Add(targetTile4);
            return tiles;
        }

        public List<TileBase> GetConditionTiles()
        {
            List<TileBase> tiles = new List<TileBase>();
            if (conditionCheckBuilding1 != null) tiles.Add(conditionCheckBuilding1);
            if (conditionCheckBuilding2 != null) tiles.Add(conditionCheckBuilding2);
            if (conditionCheckBuilding3 != null) tiles.Add(conditionCheckBuilding3);
            if (conditionCheckBuilding4 != null) tiles.Add(conditionCheckBuilding4);
            return tiles;
        }
    }

    [System.Serializable]
    public class GlobalCondition
    {
        public bool enabled = false;
        public ConditionType conditionType;
        public TileBase conditionCheckBuilding1;
        public TileBase conditionCheckBuilding2;
        public TileBase conditionCheckBuilding3;
        public TileBase conditionCheckBuilding4;
        public int conditionRadius = 1;
        public int requiredCount = 1;
        public bool inverseCondition = false;

        public enum ConditionType
        {
            None,
            DestroyOnlyIfNoNearbyBuildings,
            DestroyOnlyIfNearbyBuildings,
            DestroyOnlyIfNearbyBuildingsCount
        }

        public List<TileBase> GetConditionTiles()
        {
            List<TileBase> tiles = new List<TileBase>();
            if (conditionCheckBuilding1 != null) tiles.Add(conditionCheckBuilding1);
            if (conditionCheckBuilding2 != null) tiles.Add(conditionCheckBuilding2);
            if (conditionCheckBuilding3 != null) tiles.Add(conditionCheckBuilding3);
            if (conditionCheckBuilding4 != null) tiles.Add(conditionCheckBuilding4);
            return tiles;
        }
    }

    private bool isDemolishMode = false;
    private Vector3Int currentHoverCell;
    private TileBase currentHoverTile;
    private BuildingRule currentRule;
    private List<Vector3Int> cellsToDestroy = new List<Vector3Int>();
    private List<Vector3Int> currentHighlightedCells = new List<Vector3Int>();
    private Camera mainCamera;
    private BuildingFireManager fireManager;

    void Start()
    {
        mainCamera = Camera.main;
        fireManager = FindObjectOfType<BuildingFireManager>();
    }

    void Update()
    {
        if (!isDemolishMode) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DisableDemolishMode();
            return;
        }

        HandleHover();
        HandleDemolish();
    }

    void HandleHover()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = buildingsTilemap.WorldToCell(mouseWorldPos);

        if (cellPosition != currentHoverCell)
        {
            ClearHighlights();
            currentHoverCell = cellPosition;
            currentHoverTile = buildingsTilemap.GetTile(cellPosition);
            currentRule = GetRuleForTile(currentHoverTile);

            if (currentRule != null)
            {
                ShowHighlights();
            }
        }
    }

    BuildingRule GetRuleForTile(TileBase tile)
    {
        foreach (var rule in buildingRules)
        {
            if (rule.targetBuilding == tile)
                return rule;
        }
        return null;
    }

    void ShowHighlights()
    {
        cellsToDestroy.Clear();
        currentHighlightedCells.Clear();

        if (!CheckGlobalCondition(currentRule, currentHoverCell))
            return;

        AddHighlight(currentHoverCell, mainHighlightTile);
        cellsToDestroy.Add(currentHoverCell);

        HashSet<Vector3Int> allCellsToDestroy = new HashSet<Vector3Int>();
        allCellsToDestroy.Add(currentHoverCell);

        CollectAllBuildingsToDestroy(currentHoverCell, currentRule, allCellsToDestroy, 0);

        foreach (var cell in allCellsToDestroy)
        {
            if (cell != currentHoverCell)
            {
                AddHighlight(cell, chainHighlightTile);
                cellsToDestroy.Add(cell);
            }
        }
    }

    void CollectAllBuildingsToDestroy(Vector3Int sourceCell, BuildingRule sourceRule, HashSet<Vector3Int> collectedCells, int depth)
    {
        if (depth > maxChainDepth) return;

        foreach (var destroyTarget in sourceRule.destroyWithIt)
        {
            List<Vector3Int> foundCells = FindBuildingsToDestroy(sourceCell, destroyTarget);

            foreach (var cell in foundCells)
            {
                if (collectedCells.Contains(cell)) continue;

                collectedCells.Add(cell);

                BuildingRule targetRule = GetRuleForTile(buildingsTilemap.GetTile(cell));
                if (targetRule != null && targetRule.destroyWithIt.Count > 0)
                {
                    CollectAllBuildingsToDestroy(cell, targetRule, collectedCells, depth + 1);
                }
            }
        }
    }

    List<Vector3Int> FindBuildingsToDestroy(Vector3Int centerCell, DestroyTarget destroyTarget)
    {
        List<Vector3Int> foundBuildings = new List<Vector3Int>();
        List<TileBase> targetTiles = destroyTarget.GetTargetTiles();

        if (targetTiles.Count == 0) return foundBuildings;

        for (int x = -destroyTarget.searchRadius; x <= destroyTarget.searchRadius; x++)
        {
            for (int y = -destroyTarget.searchRadius; y <= destroyTarget.searchRadius; y++)
            {
                Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(checkCell);

                if (tile != null && targetTiles.Contains(tile))
                {
                    if (CheckTargetCondition(checkCell, destroyTarget, centerCell))
                    {
                        foundBuildings.Add(checkCell);
                    }
                }
            }
        }

        return foundBuildings;
    }

    bool CheckTargetCondition(Vector3Int cell, DestroyTarget destroyTarget, Vector3Int sourceCell)
    {
        if (!destroyTarget.requireCondition) return true;

        List<TileBase> conditionTiles = destroyTarget.GetConditionTiles();
        bool result = true;

        switch (destroyTarget.conditionType)
        {
            case DestroyTarget.ConditionType.DestroyOnlyIfNoNearbyBuildings:
                result = !IsBuildingNearby(cell, conditionTiles, destroyTarget.conditionRadius, sourceCell);
                break;

            case DestroyTarget.ConditionType.DestroyOnlyIfNearbyBuildings:
                result = IsBuildingNearby(cell, conditionTiles, destroyTarget.conditionRadius, sourceCell);
                break;

            case DestroyTarget.ConditionType.DestroyOnlyIfNearbyBuildingsCount:
                int count = CountBuildingsNearby(cell, conditionTiles, destroyTarget.conditionRadius, sourceCell);
                result = count >= destroyTarget.requiredCount;
                break;

            default:
                result = true;
                break;
        }

        if (destroyTarget.inverseCondition)
            return !result;
        else
            return result;
    }

    bool CheckGlobalCondition(BuildingRule rule, Vector3Int cell)
    {
        if (!rule.useGlobalCondition) return true;
        if (!rule.globalCondition.enabled) return true;

        List<TileBase> conditionTiles = rule.globalCondition.GetConditionTiles();
        bool result = true;

        switch (rule.globalCondition.conditionType)
        {
            case GlobalCondition.ConditionType.DestroyOnlyIfNoNearbyBuildings:
                result = !IsBuildingNearby(cell, conditionTiles, rule.globalCondition.conditionRadius, cell);
                break;

            case GlobalCondition.ConditionType.DestroyOnlyIfNearbyBuildings:
                result = IsBuildingNearby(cell, conditionTiles, rule.globalCondition.conditionRadius, cell);
                break;

            case GlobalCondition.ConditionType.DestroyOnlyIfNearbyBuildingsCount:
                int count = CountBuildingsNearby(cell, conditionTiles, rule.globalCondition.conditionRadius, cell);
                result = count >= rule.globalCondition.requiredCount;
                break;

            default:
                result = true;
                break;
        }

        if (rule.globalCondition.inverseCondition)
            return !result;
        else
            return result;
    }

    bool IsBuildingNearby(Vector3Int center, List<TileBase> buildingsToCheck, int radius, Vector3Int excludeCell)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3Int checkCell = center + new Vector3Int(x, y, 0);

                if (checkCell == excludeCell) continue;

                TileBase tile = buildingsTilemap.GetTile(checkCell);

                if (tile != null && buildingsToCheck.Contains(tile))
                    return true;
            }
        }
        return false;
    }

    int CountBuildingsNearby(Vector3Int center, List<TileBase> buildingsToCheck, int radius, Vector3Int excludeCell)
    {
        int count = 0;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3Int checkCell = center + new Vector3Int(x, y, 0);

                if (checkCell == excludeCell) continue;

                TileBase tile = buildingsTilemap.GetTile(checkCell);

                if (tile != null && buildingsToCheck.Contains(tile))
                    count++;
            }
        }
        return count;
    }

    void AddHighlight(Vector3Int cell, TileBase highlightTile)
    {
        highlightTilemap.SetTile(cell, highlightTile);
        currentHighlightedCells.Add(cell);
    }

    void ClearHighlights()
    {
        foreach (var cell in currentHighlightedCells)
        {
            highlightTilemap.SetTile(cell, null);
        }
        currentHighlightedCells.Clear();
    }

    void HandleDemolish()
    {
        if (Input.GetKeyDown(demolishKey) && currentRule != null && cellsToDestroy.Count > 0)
        {
            ExecuteDemolition();
        }
    }

    void ExecuteDemolition()
    {
        foreach (var cell in cellsToDestroy)
        {
            buildingsTilemap.SetTile(cell, null);
            highlightTilemap.SetTile(cell, null);

            if (fireManager != null)
                fireManager.ClearFire(cell);
        }

        ClearHighlights();
        currentRule = null;
        cellsToDestroy.Clear();
    }

    public void DestroySingleBuilding(Vector3Int cell)
    {
        TileBase tile = buildingsTilemap.GetTile(cell);
        if (tile == null) return;

        BuildingRule rule = GetRuleForTile(tile);

        if (rule != null && rule.destroyWithIt.Count > 0)
        {
            HashSet<Vector3Int> allCells = new HashSet<Vector3Int>();
            allCells.Add(cell);
            CollectAllBuildingsToDestroy(cell, rule, allCells, 0);

            foreach (var c in allCells)
            {
                buildingsTilemap.SetTile(c, null);
                if (fireManager != null)
                    fireManager.ClearFire(c);
            }
        }
        else
        {
            buildingsTilemap.SetTile(cell, null);
            if (fireManager != null)
                fireManager.ClearFire(cell);
        }
    }

    public void EnableDemolishMode()
    {
        isDemolishMode = true;
    }

    public void DisableDemolishMode()
    {
        isDemolishMode = false;
        ClearHighlights();
    }

    public void ToggleDemolishMode()
    {
        if (isDemolishMode)
            DisableDemolishMode();
        else
            EnableDemolishMode();
    }

    public bool IsDemolishMode()
    {
        return isDemolishMode;
    }
}