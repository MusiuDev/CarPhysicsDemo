using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class DynamicPoolProvider
{
    private static Dictionary<IPoolable, DynamicPool> _activePools = new Dictionary<IPoolable, DynamicPool>();

    public static T Get<T>(T poolable) where T : Component, IPoolable
    {
        if (!_activePools.TryGetValue(poolable, out DynamicPool pool))
        {
            pool = new GameObject($"Pool<{poolable.GameObjectReference.name}>").AddComponent<DynamicPool>();
            pool.Initialize(poolable);
            pool.OnPoolDestroyed += () => HandlePoolDestroyed(pool);
            _activePools.Add(poolable, pool);
        }

        var instance = (T)pool.GetInstance();
        return instance;
    }

    private static void HandlePoolDestroyed(DynamicPool pool)
    {
        _activePools.Remove(pool.Prefab);
    }

    public static void Return<T>(T poolable) where T : Component, IPoolable
    {
        if (!poolable)
        {
            Debug.LogError("Cannot return a null poolable");
            return;
        }
        if (!poolable.ParentPool)
        {
            Debug.LogError($"Trying to return an orphaned poolable {poolable}");
            return;
        }
        poolable.ParentPool.ReturnInstance(poolable);
    }
}
