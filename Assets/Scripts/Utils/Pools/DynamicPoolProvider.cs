using System.Collections.Generic;
using UnityEngine;

public static class DynamicPoolProvider
{
    private static Dictionary<IPoolable, IDynamicPool> _activePools = new Dictionary<IPoolable, IDynamicPool>();

    public static T Get<T>(T poolable) where T : Component, IPoolable
    {
        if (!_activePools.TryGetValue(poolable, out IDynamicPool pool))
        {
            pool = new DynamicPool<T>(poolable);
            _activePools.Add(poolable, pool);
        }
        return (T)pool.GetInstance();
    }

    public static void Return<T>(T poolable) where T : Component, IPoolable
    {
        if (poolable) poolable.ParentPool.ReturnInstance(poolable);
    }

    public static void Clear()
    {
        foreach (var item in _activePools)
        {
            item.Value.Destroy();
        }
        _activePools.Clear();
    }
}
