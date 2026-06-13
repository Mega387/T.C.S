using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Ссылки на системы")]
    [SerializeField] private BuildingFireManager fireManager;
    [SerializeField] private DemolitionSystem demolitionSystem;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private TilemapRestrictionEnemy enemyRestriction;

    [Header("Настройки King")]
    [SerializeField] private TileBase kingTile;
    [SerializeField] private float kingHealth = 100f;
    private float currentKingHealth;

    private bool isGameOver = false;

    private void Start()
    {
        currentKingHealth = kingHealth;
    }

    public void DamageKing(float damage)
    {
        if (isGameOver) return;

        currentKingHealth -= damage;

        if (currentKingHealth <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        if (waveSpawner != null)
            waveSpawner.GameEnded();

        Debug.Log("GAME OVER - King destroyed!");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public TileBase GetKingTile()
    {
        return kingTile;
    }
}