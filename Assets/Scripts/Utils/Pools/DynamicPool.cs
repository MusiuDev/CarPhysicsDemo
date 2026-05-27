using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class DynamicPool : MonoBehaviour
{
    public delegate void PoolEvent();
    public event PoolEvent OnPoolDestroyed;

    private IPoolable _prefab;
    public IPoolable Prefab => _prefab;
    private HashSet<IPoolable> _instances = new HashSet<IPoolable>();
    private Stack<IPoolable> _availableInstances = new Stack<IPoolable>();

    public void Initialize(IPoolable prefab)
    {
        _prefab = prefab;
    }

    public IPoolable GetInstance()
    {
        IPoolable selected;

        if (_availableInstances.Count == 0)
        {
            selected = _prefab.CreateInstance(this);
            _instances.Add(selected);
        }
        else
        {
            selected = _availableInstances.Pop();
        }

        selected.OnPooledSpawn();
        return selected;
    }

    public void ReturnInstance(IPoolable instance)
    {
        if (instance == null) return;

        if (!_instances.Contains(instance))
        {
            Debug.LogError("Trying to return the wrong object to the pool.");
            return;
        }

        _availableInstances.Push(instance);
        instance.OnPooledDespawn();
    }

    public void OnDestroy()
    {
        _instances.Clear();
        _availableInstances.Clear();
        OnPoolDestroyed?.Invoke();
    }
}

public interface IPoolable
{
    GameObject GameObjectReference { get; }
    DynamicPool ParentPool { get; }
    IPoolable CreateInstance(DynamicPool parent);
    void OnPooledSpawn();
    void OnPooledDespawn();
}