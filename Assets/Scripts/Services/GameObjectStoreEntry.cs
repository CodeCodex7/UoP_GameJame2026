using UnityEngine;

public class GameObjectStoreEntry : MonoBehaviour
{
    [SerializeField] private string key = "Units";

    public string Key => key;

    private void OnEnable()
    {
        Services.ResolveWhenValid<GameObjectStoreService>(Register);
    }

    private void OnDisable()
    {
        if (Services.TryResolve<GameObjectStoreService>(out var store))
        {
            store.Unregister(gameObject, key);
        }
    }

    private void Register()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        var store = Services.Resolve<GameObjectStoreService>();

        if (store != null)
        {
            store.Register(gameObject, key);
        }
    }
}
