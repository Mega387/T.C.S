using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class KingProtectionSystem : MonoBehaviour
{
    [Header("Настройки King")]
    [SerializeField] private Tilemap buildingsTilemap;
    [SerializeField] private TileBase kingTile;
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private float startDelay = 2f;

    [Header("Настройки логовов врага")]
    [SerializeField] private Tilemap enemyBuildingsTilemap;
    [SerializeField] private TileBase enemyLairTile;

    [Header("UI Панели")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("Общие настройки")]
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private float screenDuration = 5f;
    [SerializeField] private bool pauseGameOnEnd = true;
    [SerializeField] private AudioClip endGameSound;

    private bool isKingAlive = true;
    private bool hasGameEnded = false;
    private bool isVictory = false;
    private Vector3Int kingCell;
    private bool kingFound = false;
    private Coroutine checkCoroutine;

    private void Start()
    {
        defeatPanel?.SetActive(false);
        victoryPanel?.SetActive(false);

        if (buildingsTilemap == null)
        {
            buildingsTilemap = FindObjectOfType<BuildingFireManager>()?.buildingsTilemap;
        }

        if (kingTile == null && buildingsTilemap != null)
        {
            BuildingFireManager fireManager = FindObjectOfType<BuildingFireManager>();
            if (fireManager != null)
            {
                kingTile = fireManager.kingTile;
            }
        }

        if (enemyBuildingsTilemap == null)
        {
            GameObject enemyTilemapObj = GameObject.Find("EnemyTilemap");
            if (enemyTilemapObj != null)
            {
                enemyBuildingsTilemap = enemyTilemapObj.GetComponent<Tilemap>();
            }
        }

        StartCoroutine(InitializeWithDelay());
    }

    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(startDelay);

        FindKingPosition();

        if (kingFound)
        {
            checkCoroutine = StartCoroutine(CheckGameStatus());
        }
        else
        {
            StartCoroutine(RetryFindKing());
        }
    }

    private IEnumerator RetryFindKing()
    {
        int retryCount = 0;
        int maxRetries = 10;

        while (!kingFound && retryCount < maxRetries)
        {
            yield return new WaitForSeconds(0.5f);
            FindKingPosition();
            retryCount++;
        }

        if (kingFound)
        {
            checkCoroutine = StartCoroutine(CheckGameStatus());
        }
        else
        {
            Debug.LogError("KingProtectionSystem: King не найден после нескольких попыток!");
        }
    }

    private void FindKingPosition()
    {
        if (buildingsTilemap == null || kingTile == null) return;

        BoundsInt bounds = buildingsTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = buildingsTilemap.GetTile(cell);
                if (tile == kingTile)
                {
                    kingCell = cell;
                    kingFound = true;
                    Debug.Log("King найден на позиции: " + kingCell);
                    return;
                }
            }
        }
    }

    private IEnumerator CheckGameStatus()
    {
        while (!hasGameEnded)
        {
            yield return new WaitForSeconds(checkInterval);

            if (!isVictory && CheckAllEnemyLairsDestroyed())
            {
                isVictory = true;
                OnVictory();
                yield break;
            }

            if (kingFound && buildingsTilemap != null)
            {
                TileBase currentTile = buildingsTilemap.GetTile(kingCell);

                if (currentTile != kingTile)
                {
                    isKingAlive = false;
                    OnDefeat();
                    yield break;
                }
            }
        }
    }

    private bool CheckAllEnemyLairsDestroyed()
    {
        if (enemyBuildingsTilemap == null || enemyLairTile == null)
        {
            return false;
        }

        BoundsInt bounds = enemyBuildingsTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = enemyBuildingsTilemap.GetTile(cell);
                if (tile == enemyLairTile)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void OnDefeat()
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }

        Debug.Log("King уничтожен! Игра проиграна.");

        PlayEndSound();

        if (pauseGameOnEnd)
        {
            Time.timeScale = 0f;
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private void OnVictory()
    {
        if (hasGameEnded) return;
        hasGameEnded = true;

        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }

        Debug.Log("Все логова врага уничтожены! Игра выиграна!");

        PlayEndSound();

        if (pauseGameOnEnd)
        {
            Time.timeScale = 0f;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        StartCoroutine(ReturnToMenuAfterDelay());
    }

    private void PlayEndSound()
    {
        if (endGameSound != null)
        {
            AudioSource.PlayClipAtPoint(endGameSound, Camera.main.transform.position);
        }
    }

    private IEnumerator ReturnToMenuAfterDelay()
    {
        float startTime = Time.unscaledTime;
        float elapsed = 0f;

        while (elapsed < screenDuration)
        {
            elapsed = Time.unscaledTime - startTime;
            yield return null;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public bool IsKingAlive()
    {
        return isKingAlive;
    }

    public Vector3Int GetKingPosition()
    {
        return kingCell;
    }

    public void ManualCheckKing()
    {
        if (!hasGameEnded && kingFound)
        {
            TileBase currentTile = buildingsTilemap?.GetTile(kingCell);
            if (currentTile != kingTile)
            {
                isKingAlive = false;
                OnDefeat();
            }
        }
    }

    public void ManualCheckVictory()
    {
        if (!hasGameEnded && !isVictory && CheckAllEnemyLairsDestroyed())
        {
            isVictory = true;
            OnVictory();
        }
    }

    private void OnDestroy()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
    }
}