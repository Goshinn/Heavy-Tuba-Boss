using System.Collections;
using UnityEngine;

public class EnemyDespawner : MonoBehaviour
{
    public void BeginDespawn(float despawnTime)
    {
        StartCoroutine(DelayScriptActivationThenDespawnGameObject(despawnTime));
    }

    private IEnumerator DelayScriptActivationThenDespawnGameObject(float despawnTime)
    {
        float elapsed = 0;

        // Destroy gameobject after despawnTime has been reached
        while (elapsed < despawnTime)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}