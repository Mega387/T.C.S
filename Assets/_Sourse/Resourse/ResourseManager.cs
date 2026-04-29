using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    private Dictionary<Tile, int> tileCounts = new Dictionary<Tile, int>();

    void Start()
    {
        StartCoroutine(ResourceTick());
    }

    void Update()
    {
        CountTiles();
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

    IEnumerator ResourceTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(40f);

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
        }
    }
}