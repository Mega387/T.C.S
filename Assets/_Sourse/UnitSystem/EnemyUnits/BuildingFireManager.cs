using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BuildingFireManager : MonoBehaviour
{
    [SerializeField] public Tilemap buildingsTilemap;
    [SerializeField] public Tilemap fireTilemap;

    [SerializeField] private TileBase fireStage1;
    [SerializeField] private TileBase fireStage2;
    [SerializeField] private TileBase fireStage3;
    [SerializeField] private TileBase fireStage4;
    [SerializeField] private TileBase fireStage5;
    [SerializeField] private TileBase fireStage6;

    [SerializeField] public TileBase kingTile;
    [SerializeField] private List<TileBase> stoneWallTiles;

    [SerializeField] private float fireDecayDelay = 30f;
    [SerializeField] private float fireDecayInterval = 10f;

    private Dictionary<Vector3Int, int> activeFires = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, float> lastAttackTime = new Dictionary<Vector3Int, float>();
    private Dictionary<Vector3Int, Coroutine> decayCoroutines = new Dictionary<Vector3Int, Coroutine>();

    private void Start()
    {
        if (fireTilemap == null)
        {
            fireTilemap = GetComponent<Tilemap>();
        }  
    }

    public int AddFireStage(Vector3Int cell, TileBase buildingTile)
    {
        int currentStage = 0;

        if (activeFires.ContainsKey(cell))
        {
            currentStage = activeFires[cell];
            lastAttackTime[cell] = Time.time;

            if (decayCoroutines.ContainsKey(cell))
            {
                StopCoroutine(decayCoroutines[cell]);
                decayCoroutines.Remove(cell);
            }

            if (currentStage < 6)
            {
                currentStage++;
                activeFires[cell] = currentStage;
                UpdateFireTile(cell, currentStage);
            }

            StartDecayCoroutine(cell);
        }
        else
        {
            currentStage = 1;
            activeFires[cell] = currentStage;
            lastAttackTime[cell] = Time.time;
            UpdateFireTile(cell, currentStage);
            StartDecayCoroutine(cell);
        }

        return currentStage;
    }

    private void StartDecayCoroutine(Vector3Int cell)
    {
        if (decayCoroutines.ContainsKey(cell))
        {
            if (decayCoroutines[cell] != null)
                StopCoroutine(decayCoroutines[cell]);
            decayCoroutines.Remove(cell);
        }

        Coroutine coroutine = StartCoroutine(DecayFire(cell));
        decayCoroutines[cell] = coroutine;
    }

    public int GetFireStage(Vector3Int cell)
    {
        if (activeFires.ContainsKey(cell))
        {
            return activeFires[cell];
        }
        return 0;
    }

    private IEnumerator DecayFire(Vector3Int cell)
    {
        while (activeFires.ContainsKey(cell))
        {
            yield return new WaitForSeconds(fireDecayInterval);

            if (!activeFires.ContainsKey(cell)) break;

            float timeSinceLastAttack = Time.time - lastAttackTime[cell];

            if (timeSinceLastAttack >= fireDecayDelay)
            {
                int newStage = activeFires[cell] - 1;

                if (newStage <= 0)
                {
                    ClearFire(cell);
                    break;
                }
                else
                {
                    activeFires[cell] = newStage;
                    UpdateFireTile(cell, newStage);



                }
            }
        }
    }

    private void UpdateFireTile(Vector3Int cell, int stage)
    {
        TileBase fireTile = GetFireTileByStage(stage);
        if (fireTile != null)
        {
            fireTilemap.SetTile(cell, fireTile);
        }
    }

    private TileBase GetFireTileByStage(int stage)
    {
        switch (stage)
        {
            case 1: return fireStage1;
            case 2: return fireStage2;
            case 3: return fireStage3;
            case 4: return fireStage4;
            case 5: return fireStage5;
            case 6: return fireStage6;
            default: return null;
        }
    }

    public void ClearFire(Vector3Int cell)
    {
        if (activeFires.ContainsKey(cell))
        {
            activeFires.Remove(cell);
            lastAttackTime.Remove(cell);
        }

        if (decayCoroutines.ContainsKey(cell))
        {
            if (decayCoroutines[cell] != null)
                StopCoroutine(decayCoroutines[cell]);
            decayCoroutines.Remove(cell);
        }

        fireTilemap.SetTile(cell, null);
    }

    public bool IsStoneWall(TileBase tile)
    {
        return stoneWallTiles.Contains(tile);
    }

    public void ClearAllFires()
    {
        foreach (var cell in activeFires.Keys)
        {
            fireTilemap.SetTile(cell, null);
        }

        foreach (var coroutine in decayCoroutines.Values)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        activeFires.Clear();
        lastAttackTime.Clear();
        decayCoroutines.Clear();
    }
}