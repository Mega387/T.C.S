using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;

public class ResourseManager : MonoBehaviour
{
    [System.Serializable]
    public class ResourceProducer
    {
        public Tile targetTile;
        public float foodPerMinute;
        public float woodPerMinute;
        public float stonePerMinute;
        public float ironOrePerMinute;
        public float ironIngotPerMinute;
        public float populationPerMinute;
    }

    [Header("Настройки ресурсов")]
    public List<ResourceProducer> producers = new List<ResourceProducer>();
    public Tilemap buildTilemap;

    [Header("геймобджекты")]
    [SerializeField] GameObject ResourseUImenu;
    [SerializeField] GameObject ResourseUImenuSpending;

    [Header("UI для отображения дохода")]
    [SerializeField] private TextMeshProUGUI foodIncomeText;
    [SerializeField] private TextMeshProUGUI woodIncomeText;
    [SerializeField] private TextMeshProUGUI stoneIncomeText;
    [SerializeField] private TextMeshProUGUI ironOreIncomeText;
    [SerializeField] private TextMeshProUGUI ironIngotIncomeText;

    [Header("Слайдер")]
    [SerializeField] private Slider progressSlider;

    private Dictionary<Tile, int> tileCounts = new Dictionary<Tile, int>();
    private float lastIncomeUpdate;
    private float currentFoodIncome;
    private float currentWoodIncome;
    private float currentStoneIncome;
    private float currentIronOreIncome;
    private float currentIronIngotIncome;
    private Coroutine resourceCoroutine;

    void Start()
    {
        resourceCoroutine = StartCoroutine(ResourceTick());
        lastIncomeUpdate = Time.time;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = 1;
            progressSlider.value = 0;
        }
    }

    public void PressButtonResourse()
    {
        ResourseUImenu.SetActive(true);
        ResourseUImenuSpending.SetActive(false);
    }

    public void PressButtonSpending()
    {
        ResourseUImenu.SetActive(false);
        ResourseUImenuSpending.SetActive(true);
    }

    void Update()
    {
        CountTiles();

        if (Time.time - lastIncomeUpdate >= 1f)
        {
            CalculateIncome();
            UpdateIncomeTexts();
            lastIncomeUpdate = Time.time;
        }

        UpdateSlider();
    }

    void CountTiles()
    {
        tileCounts.Clear();

        foreach (var position in buildTilemap.cellBounds.allPositionsWithin)
        {
            Tile tile = buildTilemap.GetTile<Tile>(position);
            if (tile != null)
            {
                if (tileCounts.ContainsKey(tile))
                    tileCounts[tile]++;
                else
                    tileCounts[tile] = 1;
            }
        }
    }

    void CalculateIncome()
    {
        currentFoodIncome = 0;
        currentWoodIncome = 0;
        currentStoneIncome = 0;
        currentIronOreIncome = 0;
        currentIronIngotIncome = 0;

        foreach (var producer in producers)
        {
            if (tileCounts.ContainsKey(producer.targetTile))
            {
                int count = tileCounts[producer.targetTile];
                currentFoodIncome += producer.foodPerMinute * count;
                currentWoodIncome += producer.woodPerMinute * count;
                currentStoneIncome += producer.stonePerMinute * count;
                currentIronOreIncome += producer.ironOrePerMinute * count;
                currentIronIngotIncome += producer.ironIngotPerMinute * count;
            }
        }
    }

    void UpdateIncomeTexts()
    {
        if (foodIncomeText != null)
            foodIncomeText.text = $"Еда: {currentFoodIncome:F1}\n/40сек";

        if (woodIncomeText != null)
            woodIncomeText.text = $"Древесина: {currentWoodIncome:F1}\n/40сек";

        if (stoneIncomeText != null)
            stoneIncomeText.text = $"Камень: {currentStoneIncome:F1}\n/40сек";

        if (ironOreIncomeText != null)
            ironOreIncomeText.text = $"Руда: {currentIronOreIncome:F1}\n/40сек";

        if (ironIngotIncomeText != null)
            ironIngotIncomeText.text = $"Слитки: {currentIronIngotIncome:F1}\n/40сек";
    }

    void UpdateSlider()
    {
        if (progressSlider != null && resourceCoroutine != null)
        {
            float elapsedTime = 0f;
            float currentTime = 0f;

            if (resourceCoroutine != null)
            {
                currentTime = Time.time - lastIncomeUpdate;
                float normalizedValue = (currentTime % 40f) / 40f;
                progressSlider.value = normalizedValue;
            }
        }
    }

    IEnumerator ResourceTick()
    {
        while (true)
        {
            float timer = 0;

            while (timer < 40f)
            {
                timer += Time.deltaTime;
                if (progressSlider != null)
                {
                    progressSlider.value = timer / 40f;
                }
                yield return null;
            }

            CountTiles();

            foreach (var producer in producers)
            {
                if (tileCounts.ContainsKey(producer.targetTile))
                {
                    int count = tileCounts[producer.targetTile];

                    ResoursUI.eat += producer.foodPerMinute * count;
                    ResoursUI.wooden += producer.woodPerMinute * count;
                    ResoursUI.stone += producer.stonePerMinute * count;
                    ResoursUI.ironOre += producer.ironOrePerMinute * count;
                    ResoursUI.ironIngot += producer.ironIngotPerMinute * count;
                }
            }

            ResoursUI.eat = Mathf.Max(ResoursUI.eat, 0);
            ResoursUI.wooden = Mathf.Max(ResoursUI.wooden, 0);
            ResoursUI.stone = Mathf.Max(ResoursUI.stone, 0);
            ResoursUI.ironOre = Mathf.Max(ResoursUI.ironOre, 0);
            ResoursUI.ironIngot = Mathf.Max(ResoursUI.ironIngot, 0);

            if (progressSlider != null)
            {
                progressSlider.value = 0;
            }
        }
    }
}