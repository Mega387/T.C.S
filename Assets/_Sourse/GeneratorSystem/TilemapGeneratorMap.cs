using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapGeneratorMap : MonoBehaviour
{
    [Header("сам тайл мап")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap BuildMap;

    [Header("тайлы обьектов")]
    [SerializeField] private TileBase groundTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase stoneTile;

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

    [Header("Размер карты")]
    [SerializeField] private int cordX = 30;
    [SerializeField] private int cordY = 50;

    void Start()
    {
        Ground();
        Water();
        FixWater();
        Mountains();
        FixMountaints();
        roundOffWater();
        generatorTree();
    }
    static int Randomm(int ran, int nar)
    {
        int random = Random.Range(ran, nar);
        return random;
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
                        BuildMap.SetTile(new Vector3Int(i, j, 0), treeN1);
                    }
                    else
                    {
                        BuildMap.SetTile(new Vector3Int(i, j, 0), treeN2);
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
        for (int i = 0; i <= 10; i++)
        {
            int x = Randomm(1, cordX);
            int y = Randomm(1, cordY);
            groundTilemap.SetTile(new Vector3Int(x, y, 0), stoneTile);
            int leghtstone = Randomm(7, 17);
            for (int j = 0; j <= leghtstone; j++)
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
                groundTilemap.SetTile(new Vector3Int(x, y, 0), stoneTile);
                if (x > 1 && x < cordX && y > 1 && y < cordY)
                {
                    groundTilemap.SetTile(new Vector3Int(x + 1, y + 1, 0), stoneTile);

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
                    if (groundTilemap.GetTile(new Vector3Int(i, j + 1, 0)) == stoneTile)
                    {
                        if (groundTilemap.GetTile(new Vector3Int(i, j - 1, 0)) == stoneTile)
                        {
                            if (groundTilemap.GetTile(new Vector3Int(i + 1, j, 0)) == stoneTile)
                            {
                                if (groundTilemap.GetTile(new Vector3Int(i - 1, j, 0)) == stoneTile)
                                {
                                    groundTilemap.SetTile(new Vector3Int(i, j, 0), stoneTile);
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
        for (int i = 0; i <= 10; i++)
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