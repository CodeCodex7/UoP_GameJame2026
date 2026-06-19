using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnInfo
{
    public SpawnableType SpawnType;
    public List<Spawnable> Catalogue;
    public BaseSpawnHandler SpawnHandler;
    public GameObject ObjectParent;
}