
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnableBuilding : Spawnable
{

    private Dictionary<MeshRenderer, Material[]> m_savedMaterials;


    private void Awake()
    {
        m_savedMaterials = new Dictionary<MeshRenderer, Material[]>(GhostRenderers.Select(
            x => new KeyValuePair<MeshRenderer, Material[]>(x, x.materials)));
    }

    public void SetGhostMode(bool ghostOn, Material ghostMat)
    {
        if (ghostOn)
        {
            foreach (var (r, matArray) in m_savedMaterials)
            {
                r.sharedMaterials = matArray.Select(x => ghostMat).ToArray();
            }
        }
        else
        {
            foreach (var (r, matArray) in m_savedMaterials)
            {
                r.sharedMaterials = matArray;
            }
        }
    }
}