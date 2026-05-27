using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class DynamicPool<T> : IDynamicPool where T : Component, IPoolable
{
    private T _prefab;
    private List<T> _instances = new List<T>();
    private Stack<T> _availableInstances = new Stack<T>();

    public DynamicPool(T prefab)
    {
        _prefab = prefab;
    }

    IPoolable IDynamicPool.GetInstance() => GetInstance();

    public T GetInstance()
    {
        if (_availableInstances.Count == 0)
        {
            T instanced = Object.Instantiate(_prefab);
            Object.DontDestroyOnLoad(instanced);
            _instances.Add(instanced);
            instanced.ParentPool = this;
            return instanced;
        }
        else
        {
            T selected = _availableInstances.Pop();
            selected.OnPooledSpawn();
            return selected;
        }
    }

    void IDynamicPool.ReturnInstance(IPoolable poolable) => ReturnInstance((T)poolable);

    public void ReturnInstance(T instance)
    {
        if (!instance) return;
        if (!_instances.Contains(instance))
        {
            Debug.LogError("Trying to return the wrong object to the pool.");
            return;
        }
        _availableInstances.Push(instance);
        instance.OnPooledDespawn();
    }

    public void Destroy()
    {
        foreach (var item in _instances)
        {
            if (item) item.OnPooledDestroy();
        }
        _instances.Clear();
        _availableInstances.Clear();
    }
}

public interface IDynamicPool
{
    IPoolable GetInstance();
    void ReturnInstance(IPoolable poolable);
    void Destroy();
}
public interface IPoolable
{
    IDynamicPool ParentPool { get; set; }
    void OnPooledSpawn();
    void OnPooledDespawn();
    void OnPooledDestroy();
}