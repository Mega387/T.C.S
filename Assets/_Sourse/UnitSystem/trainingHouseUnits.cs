using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

[System.Serializable]
public class UnitProduction
{
    public string unitName;
    public GameObject unitPrefab;
    public float productionTime;
    public float woodCost;
    public float eatCost;
    public float stoneCost;
    public float ironIngotCost;
    public float ironOreCost;
}

public class trainingHouseUnits : MonoBehaviour
{
    [Header("тайлмап")]
    public Tilemap buildTilemap;
    public TileBase trainingHouseTile;

    [Header("производство")]
    public List<UnitProduction> availableUnits = new List<UnitProduction>();
    public Slider productionSliderPrefab;
    public Transform worldCanvas;

    private Dictionary<Vector3Int, TrainingHouseData> activeHouses = new Dictionary<Vector3Int, TrainingHouseData>();
    private TrainingHouseData currentSelectedHouse;

    public class TrainingHouseData
    {
        public Vector3Int tilePos;
        public Vector3 worldPos;
        public bool isProductionActive;
        public float currentProgress;
        public UnitProduction currentUnit;
        public Slider sliderInstance;
        public Coroutine productionCoroutine;

        public TrainingHouseData(Vector3Int pos, Vector3 world)
        {
            tilePos = pos;
            worldPos = world;
            isProductionActive = false;
            currentProgress = 0f;
            currentUnit = null;
            sliderInstance = null;
            productionCoroutine = null;
        }
    }

    void Update()
    {
        if (buildTilemap == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTilePos = buildTilemap.WorldToCell(mouseWorldPos);
            TileBase clickedTile = buildTilemap.GetTile(clickedTilePos);

            if (clickedTile != null && clickedTile == trainingHouseTile)
            {
                if (!activeHouses.ContainsKey(clickedTilePos))
                {
                    Vector3 worldPos = buildTilemap.CellToWorld(clickedTilePos);
                    TrainingHouseData newHouse = new TrainingHouseData(clickedTilePos, worldPos);
                    activeHouses.Add(clickedTilePos, newHouse);
                }

                TrainingHouseData house = activeHouses[clickedTilePos];
                if (!house.isProductionActive)
                {
                    currentSelectedHouse = house;

                    if (TrainingHouseUIManager.Instance != null)
                    {
                        TrainingHouseUIManager.Instance.OpenUnitSelectionMenu(this);
                    }
                }
            }
        }
    }

    public List<UnitProduction> GetAvailableUnits()
    {
        return availableUnits;
    }

    public void StartProductionOnHouse(int unitIndex)
    {
        if (currentSelectedHouse == null) return;
        if (currentSelectedHouse.isProductionActive) return;
        if (unitIndex < 0 || unitIndex >= availableUnits.Count) return;

        UnitProduction unit = availableUnits[unitIndex];

        if (!CheckResources(unit)) return;

        SpendResources(unit);

        currentSelectedHouse.currentUnit = unit;
        currentSelectedHouse.isProductionActive = true;
        currentSelectedHouse.currentProgress = 0f;

        if (productionSliderPrefab != null && worldCanvas != null)
        {
            Slider slider = Instantiate(productionSliderPrefab, worldCanvas);
            slider.transform.position = currentSelectedHouse.worldPos + new Vector3(0.5f, 1, 0);
            slider.maxValue = unit.productionTime;
            slider.value = 0;
            currentSelectedHouse.sliderInstance = slider;
        }

        currentSelectedHouse.productionCoroutine = StartCoroutine(ProductionProcess(currentSelectedHouse));
    }

    IEnumerator ProductionProcess(TrainingHouseData house)
    {
        while (house.currentProgress < house.currentUnit.productionTime)
        {
            house.currentProgress += Time.deltaTime;
            if (house.sliderInstance != null)
                house.sliderInstance.value = house.currentProgress;
            yield return null;
        }

        Instantiate(house.currentUnit.unitPrefab, house.worldPos, Quaternion.identity);
        ResetHouseProduction(house);
    }

    void ResetHouseProduction(TrainingHouseData house)
    {
        house.isProductionActive = false;
        house.currentUnit = null;
        house.currentProgress = 0f;

        if (house.sliderInstance != null)
            Destroy(house.sliderInstance.gameObject);

        if (house.productionCoroutine != null)
            StopCoroutine(house.productionCoroutine);
    }

    public bool CheckResources(UnitProduction unit)
    {
        return ResoursUI.wooden >= unit.woodCost &&
               ResoursUI.eat >= unit.eatCost &&
               ResoursUI.stone >= unit.stoneCost &&
               ResoursUI.ironIngot >= unit.ironIngotCost &&
               ResoursUI.ironOre >= unit.ironOreCost;
    }

    void SpendResources(UnitProduction unit)
    {
        ResoursUI.wooden -= unit.woodCost;
        ResoursUI.eat -= unit.eatCost;
        ResoursUI.stone -= unit.stoneCost;
        ResoursUI.ironIngot -= unit.ironIngotCost;
        ResoursUI.ironOre -= unit.ironOreCost;
    }

    public void ClearCurrentSelection()
    {
        currentSelectedHouse = null;
    }
}