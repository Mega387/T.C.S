using UnityEngine;

public class LogovoEnemyLink : MonoBehaviour
{
    private LogovoSpawner.LogovoState logovo;
    private LogovoSpawner spawner;

    public void Initialize(LogovoSpawner.LogovoState logovoState, LogovoSpawner logovoSpawner)
    {
        logovo = logovoState;
        spawner = logovoSpawner;
    }

    private void OnDestroy()
    {
        if (spawner != null && logovo != null)
        {
            spawner.OnEnemyDied(logovo, gameObject);
        }
    }
}