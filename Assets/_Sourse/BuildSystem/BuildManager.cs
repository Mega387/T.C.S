using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    private Dictionary<Vector3Int, Building> activeConstructions = new Dictionary<Vector3Int, Building>();

    [System.Serializable]
    public class BuildingRequirement
    {

        public enum RequirementType { NearBuilding, NearTile, NearSpecificTile }
        public RequirementType requirementType;
        public List<Tile> requiredTiles;
        public int requiredCount;
        public int checkRadius;
        public Tilemap checkTilemap;
    }

    [System.Serializable]
    public class Building
    {
        public string name;
        public Sprite icon;
        public Tile buildingTile;
        public List<Tile> constructionTiles;
        public float buildTime;
        public ResourceCost cost;
        public List<Tile> allowedGroundTiles;
        public List<Tile> allowedGroundTwoTiles;
        public bool allowEmptyGroundTwo = true;

        public List<BuildingRequirement> requirements;
        public GameObject buildingCard;
    }

    [System.Serializable]
    public class ResourceCost
    {
        public float wood;
        public float stone;
        public float food;
        public float ironOre;
        public float ironIngot;
        public float People;
    }

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap groundTwoTilemap;
    [SerializeField] private Tilemap buildTilemap;
    [SerializeField] private Tilemap buildEnemyTilemap;
    [SerializeField] private Tilemap previewTilemap;

    [Header("UI Elements")]
    [SerializeField] private GameObject buildMenu;
    [SerializeField] private GameObject openMenuButton;

    [Header("Buildings List")]
    [SerializeField] private List<Building> buildings = new List<Building>();

    [Header("Preview Tiles")]
    [SerializeField] private Tile validTile;
    [SerializeField] private Tile invalidTile;

    private Building selectedBuilding;
    private Vector3Int currentCellPos;
    private bool isBuildingMode = false;
    private bool isShiftPressed = false;

    void Update()
    {
        if (isBuildingMode)
        {
            HandleBuildingPreview();

            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceBuilding();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelBuilding();
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isShiftPressed = true;
        }
        else
        {
            isShiftPressed = false;
        }
    }

    public void OpenBuildMenu()
    {
        buildMenu.SetActive(true);
        openMenuButton.SetActive(false);
    }

    public void CloseBuildMenu()
    {
        buildMenu.SetActive(false);
        openMenuButton.SetActive(true);
    }

    public void SelectBuilding(int buildingIndex)
    {
        if (buildingIndex < 0 || buildingIndex >= buildings.Count) return;

        selectedBuilding = buildings[buildingIndex];
        buildMenu.SetActive(false);
        ShowBuildingCard();
    }

    void ShowBuildingCard()
    {
        foreach (var building in buildings)
        {
            if (building.buildingCard != null)
                building.buildingCard.SetActive(false);
        }

        if (selectedBuilding.buildingCard != null)
            selectedBuilding.buildingCard.SetActive(true);
    }

    public void ConfirmBuildingSelection()
    {
        if (selectedBuilding != null && selectedBuilding.buildingCard != null)
            selectedBuilding.buildingCard.SetActive(false);
        openMenuButton.SetActive(true);
        isBuildingMode = true;
    }

    public void CancelBuildingSelection()
    {
        if (selectedBuilding != null && selectedBuilding.buildingCard != null)
            selectedBuilding.buildingCard.SetActive(false);

        selectedBuilding = null;
        openMenuButton.SetActive(true);
        isBuildingMode = false;
        ClearPreview();
    }

    void HandleBuildingPreview()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = previewTilemap.WorldToCell(mouseWorldPos);

        if (cellPos != currentCellPos)
        {
            ClearPreview();
            currentCellPos = cellPos;

            bool isValid = IsPlacementValid(cellPos);
            previewTilemap.SetTile(cellPos, isValid ? validTile : invalidTile);
        }
    }

    bool IsPlacementValid(Vector3Int position)
    {
        if (buildTilemap.HasTile(position) || buildEnemyTilemap.HasTile(position))
            return false;

        Tile groundTile = groundTilemap.GetTile<Tile>(position);
        Tile groundTwoTile = groundTwoTilemap.GetTile<Tile>(position);

        bool validGround = groundTile != null && selectedBuilding.allowedGroundTiles.Contains(groundTile);
        bool validGroundTwo = false;
        if (selectedBuilding.allowedGroundTwoTiles.Count > 0)
        {
            validGroundTwo = groundTwoTile != null && selectedBuilding.allowedGroundTwoTiles.Contains(groundTwoTile);
        }
        else if (selectedBuilding.allowEmptyGroundTwo)
        {
            validGroundTwo = groundTwoTile == null;
        }
        if (groundTile != null && !validGround)
            return false;
        if (groundTwoTile != null && !validGroundTwo)
            return false;
        if (!validGround && !validGroundTwo)
            return false;

        if (!IsNearExistingBuilding(position))
            return false;

        if (!CheckBuildingRequirements(position))
            return false;

        return true;
    }

    bool IsNearExistingBuilding(Vector3Int position)
    {
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3Int checkPos = position + new Vector3Int(x, y, 0);
                if (buildTilemap.HasTile(checkPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool CheckBuildingRequirements(Vector3Int position)
    {
        foreach (var requirement in selectedBuilding.requirements)
        {
            if (!CheckRequirement(position, requirement))
                return false;
        }
        return true;
    }

    bool CheckRequirement(Vector3Int position, BuildingRequirement requirement)
    {
        int foundCount = 0;

        for (int x = -requirement.checkRadius; x <= requirement.checkRadius; x++)
        {
            for (int y = -requirement.checkRadius; y <= requirement.checkRadius; y++)
            {
                Vector3Int checkPos = position + new Vector3Int(x, y, 0);

                if (requirement.requirementType == BuildingRequirement.RequirementType.NearBuilding)
                {
                    if (buildTilemap.HasTile(checkPos))
                        foundCount++;
                }
                else if (requirement.requirementType == BuildingRequirement.RequirementType.NearTile)
                {
                    Tile groundTile = groundTilemap.GetTile<Tile>(checkPos);
                    Tile groundTwoTile = groundTwoTilemap.GetTile<Tile>(checkPos);

                    if (groundTile != null && requirement.requiredTiles.Contains(groundTile) ||
                        groundTwoTile != null && requirement.requiredTiles.Contains(groundTwoTile))
                    {
                        foundCount++;
                    }
                }
                else if (requirement.requirementType == BuildingRequirement.RequirementType.NearSpecificTile)
                {
                    if (requirement.checkTilemap != null)
                    {
                        Tile checkTile = requirement.checkTilemap.GetTile<Tile>(checkPos);
                        if (checkTile != null && requirement.requiredTiles.Contains(checkTile))
                            foundCount++;
                    }
                }
            }
        }

        return foundCount >= requirement.requiredCount;
    }

    void TryPlaceBuilding()
    {
        if (IsPlacementValid(currentCellPos))
        {
            if (HasEnoughResources(selectedBuilding.cost))
            {
                SpendResources(selectedBuilding.cost);
                StartConstruction(currentCellPos);

                if (!isShiftPressed)
                {
                    CancelBuilding();
                }
                else
                {
                    ClearPreview();
                    currentCellPos = Vector3Int.zero;
                }
            }
        }
    }

    void StartConstruction(Vector3Int position)
    {
        buildTilemap.SetTile(position, selectedBuilding.constructionTiles[0]);
        StartCoroutine(ConstructionProgress(position, selectedBuilding));
    }

    IEnumerator ConstructionProgress(Vector3Int position, Building buildingData)
    {
        float timePerStage = buildingData.buildTime / buildingData.constructionTiles.Count;

        for (int stage = 1; stage < buildingData.constructionTiles.Count; stage++)
        {
            yield return new WaitForSeconds(timePerStage);

            if (buildTilemap != null && buildingData != null && buildingData.constructionTiles.Count > stage)
            {
                buildTilemap.SetTile(position, buildingData.constructionTiles[stage]);
            }
            else
            {
                yield break;

            }
        }

        yield return new WaitForSeconds(timePerStage);

        if (buildTilemap != null && buildingData != null && buildingData.buildingTile != null)
        {

            buildTilemap.SetTile(position, buildingData.buildingTile);
            FixBuildTile fixs = FindObjectOfType<FixBuildTile>();

            if (fixs != null)
            {
                fixs.shiftCount();
            }
        }

        if (activeConstructions.ContainsKey(position))
        {
            activeConstructions.Remove(position);
        }
    }

    bool HasEnoughResources(ResourceCost cost)
    {
        ResoursUI resourceUI = FindObjectOfType<ResoursUI>();
        if (resourceUI != null)
        {
            return ResoursUI.wooden >= cost.wood &&
                   ResoursUI.stone >= cost.stone &&
                   ResoursUI.eat >= cost.food &&
                   ResoursUI.ironOre >= cost.ironOre &&
                   ResoursUI.ironIngot >= cost.ironIngot;
        }
        return true;
    }

    void SpendResources(ResourceCost cost)
    {
        ResoursUI resourceUI = FindObjectOfType<ResoursUI>();
        if (resourceUI != null)
        {
            ResoursUI.wooden -= cost.wood;
            ResoursUI.stone -= cost.stone;
            ResoursUI.eat -= cost.food;
            ResoursUI.ironOre -= cost.ironOre;
            ResoursUI.ironIngot -= cost.ironIngot;
            ResoursUI.population += cost.People;
        }
    }

    void CancelBuilding()
    {
        isBuildingMode = false;
        selectedBuilding = null;
        ClearPreview();
    }

    void ClearPreview()
    {
        previewTilemap.SetTile(currentCellPos, null);
    }
}