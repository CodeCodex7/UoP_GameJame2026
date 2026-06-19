using System.Collections.Generic;
using System.Linq;

public class SpawnManager : MonoService<SpawnManager>
{
    public List<SpawnInfo> SpawnCatalogue;
    
    private void Awake()
    {
        RegisterService();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public SpawnInfo GetSpawnForType(SpawnableType spawnType)
    {
        return SpawnCatalogue.Find(x => x.SpawnType == spawnType);
    }
}
