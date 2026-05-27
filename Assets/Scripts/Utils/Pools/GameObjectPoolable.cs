using UnityEngine;

public class GameObjectPoolable : MonoBehaviour, IPoolable
{
    public IDynamicPool ParentPool { get; set; }
    public void OnPooledSpawn()
    {
        if (gameObject) gameObject.SetActive(true);
    }

    public void OnPooledDespawn()
    {
        if (gameObject) gameObject.SetActive(false);
    }

    public void OnPooledDestroy()
    {
        if (gameObject) Destroy(gameObject);
    }
}
