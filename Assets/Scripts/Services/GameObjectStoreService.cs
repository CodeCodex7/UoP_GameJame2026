using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class GameObjectStoreService : MonoService<GameObjectStoreService>
{
    private readonly Dictionary<string, HashSet<GameObject>> objectsByKey = new Dictionary<string, HashSet<GameObject>>();

    private void Awake()
    {
        RegisterService();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public void Register(GameObject target, string key)
    {
        if (target == null || string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!objectsByKey.TryGetValue(key, out var objects))
        {
            objects = new HashSet<GameObject>();
            objectsByKey.Add(key, objects);
        }

        objects.Add(target);
    }

    public void Unregister(GameObject target, string key)
    {
        if (target == null || string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!objectsByKey.TryGetValue(key, out var objects))
        {
            return;
        }

        objects.Remove(target);

        if (objects.Count == 0)
        {
            objectsByKey.Remove(key);
        }
    }

    public IReadOnlyCollection<GameObject> GetObjects(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || !objectsByKey.TryGetValue(key, out var objects))
        {
            return System.Array.Empty<GameObject>();
        }

        return objects;
    }

    public bool TryGetFirst(string key, out GameObject target)
    {
        target = null;

        if (string.IsNullOrWhiteSpace(key) || !objectsByKey.TryGetValue(key, out var objects))
        {
            return false;
        }

        foreach (var storedObject in objects)
        {
            if (storedObject == null)
            {
                continue;
            }

            target = storedObject;
            return true;
        }

        return false;
    }
}
