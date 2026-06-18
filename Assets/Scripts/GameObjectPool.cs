using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool<T> where T : MonoBehaviour
{
    GameObject prefab;
    private Transform parent;
    ObjectPool<GameObject> pool;


    public GameObjectPool(GameObject prefab, Transform parent, int defaultSize = 20, int maxSize = 100)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new ObjectPool<GameObject>(
            CreatePooledObject,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyPooledObject,
            true,
            defaultSize,
            maxSize
        );
    }

    public T GetObject(Vector3 position)
    {
        GameObject obj = pool.Get();
        obj.transform.position = position;
        return obj.GetComponent<T>();;
    }

    public void ReleaseObject(GameObject obj)
    {
        pool.Release(obj);
    }
    
    GameObject CreatePooledObject()
    {
        GameObject newObject = GameObject.Instantiate(prefab, parent);
        return newObject;
    }

    void OnGetFromPool(GameObject pooledObject)
    {
        pooledObject.SetActive(true);
    }

    void OnReturnToPool(GameObject pooledObject)
    {
        pooledObject.SetActive(false);
    }

    void OnDestroyPooledObject(GameObject pooledObject)
    {
        GameObject.Destroy(pooledObject);
    }
}