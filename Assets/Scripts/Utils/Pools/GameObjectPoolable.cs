using UnityEngine;

public class GameObjectPoolable : MonoBehaviour, IPoolable
{
    public DynamicPool ParentPool { get; private set; }
    public GameObject GameObjectReference => gameObject;

    public IPoolable CreateInstance(DynamicPool parent)
    {
        var newInstance = Instantiate(this);
        newInstance.ParentPool = parent;
        return newInstance;
    }

    public void OnPooledSpawn()
    {
        gameObject.SetActive(true);
    }

    public void OnPooledDespawn()
    {
        gameObject.SetActive(false);
    }
}
