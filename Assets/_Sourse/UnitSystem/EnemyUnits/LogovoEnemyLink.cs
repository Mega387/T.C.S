using UnityEngine;

public class LogovoEnemyLink : MonoBehaviour
{
    private LogovoSpawner.LogovoState logovoState;
    private LogovoSpawner spawner;
    private bool isInitialized = false;

    public void Initialize(LogovoSpawner.LogovoState logovo, LogovoSpawner spawnerScript)
    {
        logovoState = logovo;
        spawner = spawnerScript;
        isInitialized = true;
    }

    private void OnDestroy()
    {
        if (isInitialized && spawner != null && logovoState != null)
        {
            spawner.RemoveEnemyFromLogovo(logovoState, gameObject);
        }
    }
}