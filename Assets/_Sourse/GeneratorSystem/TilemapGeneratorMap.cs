using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapGeneratorMap : MonoBehaviour
{
    [Header("тайл мапы")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap groundTwoMap;
    [SerializeField] private Tilemap BuildPlayer;
    [SerializeField] private Tilemap EnemyTilemap;

    [Header("тайлы построек игрока")]
    [SerializeField] private TileBase king;
    [SerializeField] private TileBase house;
    [SerializeField] private TileBase ferma;

    [Header("тайлы обьектов")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase waterTile;
    [Header("Горы")]
    [SerializeField] private TileBase stoneTile1;
    [SerializeField] private TileBase stoneTile2;
    [SerializeField] private TileBase stoneTile3;
    [SerializeField] private TileBase stoneTile4;
    [SerializeField] private TileBase stoneTile5;
    [Header("Деревья")]
    [SerializeField] private TileBase treeN1;
    [SerializeField] private TileBase treeN2;

    [Header("круглые тайл воды верх право")]
    [SerializeField] private TileBase waterGround1;
    [Header("круглые тайл воды верх лево")]
    [SerializeField] private TileBase waterGround2;
    [Header("круглые тайл воды вниз право")]
    [SerializeField] private TileBase waterGround3;
    [Header("круглые тайл воды аниз лево")]
    [SerializeField] private TileBase waterGround4;

    [Header("Нечесть")]
    [Header("Логово")]
    [SerializeField] private TileBase logovo;
    [Header("Кладбище")]
    [SerializeField] private TileBase cemetery;
    [Header("Вулкан")]
    [SerializeField] private TileBase volcanoOn;
    [SerializeField] private TileBase volcanoOff;

    [Header("Размер карты")]
     private int cordX = 100;
     private int cordY = 80;



    private Vector3Int kingPosition;
    [SerializeField] private GameObject spawnEffectPrefab;
    private Dictionary<Vector3Int, GameObject> activeEffects = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        Ground();
        Water();
        FixWater();
        Mountains();
        FixMountaints();
        roundOffWater();
        generatorTree();
        country();
        EnemyBuild();
    }
    static int Randomm(int ran, int nar)
    {
        int random = Random.Range(ran, nar);
        return random;
    }
    void EnemyBuild()
    {
        int targetLogovoCount = Random.Range(6, 8);
        int createdLogovoCount = 0;
        int maxAttempts = 500;
        int attempts = 0;

        HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>();

        while (createdLogovoCount < targetLogovoCount && attempts < maxAttempts)
        {
            attempts++;

            int randomX = Random.Range(20, 76);
            int randomY = Random.Range(20, 76);
            Vector3Int centerPos = new Vector3Int(randomX, randomY, 0);

            if (occupiedPositions.Contains(centerPos))
                continue;

            bool isAllow = groundTilemap.GetTile(centerPos) == groundTile &&
                          !groundTwoMap.HasTile(centerPos) &&
                          !BuildPlayer.HasTile(centerPos) &&
                          groundTilemap.GetTile(centerPos) != waterTile &&
                          groundTilemap.GetTile(centerPos) != waterGround1 &&
                          groundTilemap.GetTile(centerPos) != waterGround2 &&
                          groundTilemap.GetTile(centerPos) != waterGround3 &&
                          groundTilemap.GetTile(centerPos) != waterGround4 &&
                          groundTwoMap.GetTile(centerPos) != treeN1 &&
                          groundTwoMap.GetTile(centerPos) != treeN2;

            bool NoKingInRadius(int centerX, int centerY, int radius = 12)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        Vector3Int checkPos = new Vector3Int(centerX + x, centerY + y, 0);
                        if (BuildPlayer.GetTile(checkPos) == king)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            if (isAllow && NoKingInRadius(randomX, randomY))
            {
                // Устанавливаем логово
                EnemyTilemap.SetTile(centerPos, logovo);
                occupiedPositions.Add(centerPos);
                createdLogovoCount++;

                // Определяем размер логова (1-3 для разнообразия)
                int randomComplexity = Random.Range(1, 4);
                int buildingsToAdd = 0;

                if (randomComplexity == 1) buildingsToAdd = 3;      // Маленькое
                else if (randomComplexity == 2) buildingsToAdd = 5;  // Среднее
                else buildingsToAdd = 7;                             // Большое

                int addedBuildings = 0;
                int buildingAttempts = 0;
                int maxBuildingAttempts = 100;

                while (addedBuildings < buildingsToAdd && buildingAttempts < maxBuildingAttempts)
                {
                    buildingAttempts++;

                    int offsetX = Random.Range(-2, 3);
                    int offsetY = Random.Range(-2, 3);

                    if (offsetX == 0 && offsetY == 0) continue;

                    int newX = randomX + offsetX;
                    int newY = randomY + offsetY;
                    Vector3Int buildPos = new Vector3Int(newX, newY, 0);
                    if (occupiedPositions.Contains(buildPos))
                        continue;

                    bool isAllowPosition = newX >= 10 && newX <= cordX - 10 && newY >= 10 && newY <= cordY - 10 &&
                                           groundTilemap.GetTile(buildPos) == groundTile &&
                                           !groundTwoMap.HasTile(buildPos) &&
                                           !BuildPlayer.HasTile(buildPos) &&
                                           groundTilemap.GetTile(buildPos) != waterTile &&
                                           groundTilemap.GetTile(buildPos) != waterGround1 &&
                                           groundTilemap.GetTile(buildPos) != waterGround2 &&
                                           groundTilemap.GetTile(buildPos) != waterGround3 &&
                                           groundTilemap.GetTile(buildPos) != waterGround4 &&
                                           groundTwoMap.GetTile(buildPos) != treeN1 &&
                                           groundTwoMap.GetTile(buildPos) != treeN2;

                    if (isAllowPosition)
                    {
                        int randomBuild = Random.Range(0, 10);

                        if (randomBuild <= 2) // 30% шанс на дополнительное логово
                        {
                            if (createdLogovoCount < targetLogovoCount)
                            {
                                EnemyTilemap.SetTile(buildPos, logovo);
                                occupiedPositions.Add(buildPos);
                                createdLogovoCount++;
                                addedBuildings++;
                            }
                        }
                        else if (randomBuild <= 5) // 30% шанс на кладбище
                        {
                            EnemyTilemap.SetTile(buildPos, cemetery);
                            occupiedPositions.Add(buildPos);
                            addedBuildings++;
                        }
                        else // 40% шанс на вулкан
                        {
                            EnemyTilemap.SetTile(buildPos, volcanoOff);
                            occupiedPositions.Add(buildPos);
                            addedBuildings++;
                        }
                    }
                }
            }
        }

        Debug.Log($"Создано логовов: {createdLogovoCount} из целевых {targetLogovoCount}");

        if (createdLogovoCount < targetLogovoCount)
        {
            Debug.LogWarning($"Не удалось создать все логова. Создано {createdLogovoCount} из {targetLogovoCount}");
        }
    }

    void country()
    {
        bool isSpawned = false;
        int attempts = 0;
        int maxAttempts = 100;

        while (!isSpawned && attempts < maxAttempts)
        {
            attempts++;

            int x = Random.Range(15, cordX-15);
            int y = Random.Range(15, cordY-15);

            Vector3Int pos1 = new Vector3Int(x, y, 0);
            Vector3Int pos2 = new Vector3Int(x, y + 1, 0);
            Vector3Int pos3 = new Vector3Int(x + 1, y + 1, 0);

            bool canBuild =
                groundTilemap.GetTile(pos1) == groundTile && groundTwoMap.GetTile(pos1) == null &&
                groundTilemap.GetTile(pos2) == groundTile && groundTwoMap.GetTile(pos2) == null &&
                groundTilemap.GetTile(pos3) == groundTile && groundTwoMap.GetTile(pos3) == null &&
                BuildPlayer.GetTile(pos1) == null &&
                BuildPlayer.GetTile(pos2) == null &&
                BuildPlayer.GetTile(pos3) == null;

            if (canBuild)
            {
                BuildPlayer.SetTile(pos1, king);
                BuildPlayer.SetTile(pos2, house);
                BuildPlayer.SetTile(pos3, ferma);

                kingPosition = pos1;

                Arrou arrou = FindObjectOfType<Arrou>();
                if (arrou != null)
                {
                    arrou.SetTarget(BuildPlayer, kingPosition);
                }

                isSpawned = true;
            }
        }
    }
    void ShowSpawnEffect(Vector3Int tilePosition)
    {
        if (spawnEffectPrefab != null)
        {
            Vector3 worldPos = BuildPlayer.CellToWorld(tilePosition) + new Vector3(0.5f, 0.5f, 0);
            GameObject effect = Instantiate(spawnEffectPrefab, worldPos, Quaternion.identity);
            activeEffects[tilePosition] = effect;
        }
    }
    void generatorTree()
    {
        for (int i = 0; i <= cordX; i++) //право лево
        {
            for (int j = 0; j <= cordY; j++) // вверх вниз
            {
                int isAllow = Randomm(1, 101);
                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == groundTile && isAllow <= 10)
                {
                    int typeTree = Randomm(1, 3);
                    if (typeTree == 1)
                    {
                        groundTwoMap.SetTile(new Vector3Int(i, j, 0), treeN1);
                    }
                    else
                    {
                        groundTwoMap.SetTile(new Vector3Int(i, j, 0), treeN2);
                    }
                }
            }
        }
    }
    void roundOffWater()
    {
        for (int i = 0; i <= cordX; i++) //право лево
        {
            for (int j = 0; j <= cordY; j++) // вверх вниз
            {
                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i + 1, j + 1, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i + 1, j, 0)) == groundTile)
                {
                    if (groundTilemap.GetTile(new Vector3Int(i + 1, j - 1, 0)) == waterTile)
                    {
                        groundTilemap.SetTile(new Vector3Int(i + 1, j, 0), waterTile);
                    }
                    else
                    {
                        groundTilemap.SetTile(new Vector3Int(i + 1, j, 0), waterGround3);
                    }

                }

                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i + 1, j - 1, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i, j - 1, 0)) == groundTile)
                {
                    if (groundTilemap.GetTile(new Vector3Int(i - 1, j - 1, 0)) == waterTile)
                    {
                        groundTilemap.SetTile(new Vector3Int(i, j - 1, 0), waterTile);
                    }
                    else
                    {
                        groundTilemap.SetTile(new Vector3Int(i, j - 1, 0), waterGround4);
                    }

                }

                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i - 1, j - 1, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i - 1, j, 0)) == groundTile)
                {

                    if (groundTilemap.GetTile(new Vector3Int(i - 1, j + 1, 0)) == waterTile)
                    {
                        groundTilemap.SetTile(new Vector3Int(i - 1, j, 0), waterTile);
                    }
                    else
                    {
                        groundTilemap.SetTile(new Vector3Int(i - 1, j, 0), waterGround2);
                    }
                }

                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i - 1, j + 1, 0)) == waterTile && groundTilemap.GetTile(new Vector3Int(i, j + 1, 0)) == groundTile)
                {


                    if (groundTilemap.GetTile(new Vector3Int(i + 1, j + 1, 0)) == waterTile)
                    {
                        groundTilemap.SetTile(new Vector3Int(i, j + 1, 0), waterTile);
                    }
                    else
                    {
                        groundTilemap.SetTile(new Vector3Int(i, j + 1, 0), waterGround1);
                    }
                }
            }
        }
    }
    void Ground()
    {
        for (int i = 0; i <= cordX; i++)
        {
            for (int j = 0; j <= cordY; j++)
            {
                groundTilemap.SetTile(new Vector3Int(i, j, 0), groundTile);
            }
        }

    }
    void Mountains()
    {
        for (int i = 0; i <= 14; i++)
        {
            int x = Randomm(1, cordX);
            int y = Randomm(1, cordY);
            groundTwoMap.SetTile(new Vector3Int(x, y, 0), stoneTile1);
            int leghtstone = Randomm(7, 17);
            
            for (int j = 0; j <= leghtstone; j++)
            {
                int randomtile = Randomm(1, 6);
                x += Randomm(-1, 2);
                y += Randomm(-1, 2);


                if (x < 0 || x > cordX)
                {
                    continue;
                }
                if (y < 0 || y > cordY)
                {
                    continue;
                }
                if (randomtile == 1)
                {
                    groundTwoMap.SetTile(new Vector3Int(x, y, 0), stoneTile1);
                }
                if (randomtile == 2)
                {
                    groundTwoMap.SetTile(new Vector3Int(x, y, 0), stoneTile2);
                }
                if (randomtile == 3)
                {
                    groundTwoMap.SetTile(new Vector3Int(x, y, 0), stoneTile3);
                }
                if (randomtile == 4)
                {
                    groundTwoMap.SetTile(new Vector3Int(x, y, 0), stoneTile4);

                }
                if (randomtile == 5)
                {
                    groundTwoMap.SetTile(new Vector3Int(x, y, 0), stoneTile5);
                }
                if (x > 1 && x < cordX && y > 1 && y < cordY)
                {
                    if (randomtile == 1)
                    {
                        groundTwoMap.SetTile(new Vector3Int(x+1, y + 1, 0), stoneTile1);
                    }
                    if (randomtile == 2)
                    {
                        groundTwoMap.SetTile(new Vector3Int(x + 1, y + 1, 0), stoneTile2);
                    }
                    if (randomtile == 3)
                    {
                        groundTwoMap.SetTile(new Vector3Int(x + 1, y + 1, 0), stoneTile3);
                    }
                    if (randomtile == 4)
                    {
                        groundTwoMap.SetTile(new Vector3Int(x + 1, y + 1, 0), stoneTile4);

                    }
                    if (randomtile == 5)
                    {
                        groundTwoMap.SetTile(new Vector3Int(x + 1, y + 1, 0), stoneTile5);
                    }


                }
            }
        }
    }
    void FixMountaints()
    {
        for (int i = 0; i <= cordX; i++)
        {
            for (int j = 0; j <= cordY; j++)
            {
                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == groundTile)
                {
                    if (groundTwoMap.GetTile(new Vector3Int(i, j + 1, 0)) == stoneTile1 || groundTwoMap.GetTile(new Vector3Int(i, j + 1, 0)) == stoneTile2 ||
                        groundTwoMap.GetTile(new Vector3Int(i, j + 1, 0)) == stoneTile3 || groundTwoMap.GetTile(new Vector3Int(i, j + 1, 0)) == stoneTile3 ||
                        groundTwoMap.GetTile(new Vector3Int(i, j + 1, 0)) == stoneTile5)
                    {
                        if (groundTwoMap.GetTile(new Vector3Int(i, j - 1, 0)) == stoneTile1 || groundTwoMap.GetTile(new Vector3Int(i, j - 1, 0)) == stoneTile2 ||
                        groundTwoMap.GetTile(new Vector3Int(i, j - 1, 0)) == stoneTile3 || groundTwoMap.GetTile(new Vector3Int(i, j - 1, 0)) == stoneTile3 ||
                        groundTwoMap.GetTile(new Vector3Int(i, j - 1, 0)) == stoneTile5)
                        {
                            if (groundTwoMap.GetTile(new Vector3Int(i + 1, j, 0)) == stoneTile1 || groundTwoMap.GetTile(new Vector3Int(i + 1, j, 0)) == stoneTile2 ||
                        groundTwoMap.GetTile(new Vector3Int(i + 1, j , 0)) == stoneTile3 || groundTwoMap.GetTile(new Vector3Int(i + 1, j , 0)) == stoneTile3 ||
                        groundTwoMap.GetTile(new Vector3Int(i + 1, j, 0)) == stoneTile5)
                            {
                                if (groundTwoMap.GetTile(new Vector3Int(i - 1, j, 0)) == stoneTile1 || groundTwoMap.GetTile(new Vector3Int(i - 1, j, 0)) == stoneTile2 ||
                        groundTwoMap.GetTile(new Vector3Int(i - 1, j, 0)) == stoneTile3 || groundTwoMap.GetTile(new Vector3Int(i - 1, j, 0)) == stoneTile3 ||
                        groundTwoMap.GetTile(new Vector3Int(i - 1, j, 0)) == stoneTile5)
                                {
                                    int randomtile = Randomm(1, 6);
                                    if (randomtile == 1)
                                    {
                                        groundTwoMap.SetTile(new Vector3Int(i , j , 0), stoneTile1);
                                    }
                                    if (randomtile == 2)
                                    {
                                        groundTwoMap.SetTile(new Vector3Int(i , j , 0), stoneTile2);
                                    }
                                    if (randomtile == 3)
                                    {
                                        groundTwoMap.SetTile(new Vector3Int(i , j , 0), stoneTile3);
                                    }
                                    if (randomtile == 4)
                                    {
                                        groundTwoMap.SetTile(new Vector3Int(i , j , 0), stoneTile4);

                                    }
                                    if (randomtile == 5)
                                    {
                                        groundTwoMap.SetTile(new Vector3Int(i, j , 0), stoneTile5);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    void FixWater()
    {
        for (int i = 0; i <= cordX; i++)
        {
            for (int j = 0; j <= cordY; j++)
            {
                if (groundTilemap.GetTile(new Vector3Int(i, j, 0)) == groundTile)
                {
                    if (groundTilemap.GetTile(new Vector3Int(i, j + 1, 0)) == waterTile)
                    {
                        if (groundTilemap.GetTile(new Vector3Int(i, j - 1, 0)) == waterTile)
                        {
                            if (groundTilemap.GetTile(new Vector3Int(i + 1, j, 0)) == waterTile)
                            {
                                if (groundTilemap.GetTile(new Vector3Int(i - 1, j, 0)) == waterTile)
                                {
                                    groundTilemap.SetTile(new Vector3Int(i, j, 0), waterTile);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    void Water()
    {
        for (int i = 0; i <= 15; i++)
        {
            int x = Randomm(1, cordX);
            int y = Randomm(1, cordY);
            groundTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
            int leghtwater = Randomm(15, cordX - 5);
            for (int j = 0; j <= leghtwater; j++)
            {

                x += Randomm(-1, 2);
                y += Randomm(-1, 2);


                if (x < 0 || x > cordX)
                {
                    continue;
                }
                if (y < 0 || y > cordY)
                {
                    continue;
                }
                groundTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                if (x > 1 && x < cordX && y > 1 && y < cordY)
                {
                    groundTilemap.SetTile(new Vector3Int(x + 1, y + 1, 0), waterTile);

                }
            }
        }
    }
    void Update()
    {

    }
}