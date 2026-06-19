using AI.Goap.UnitAI.Behaviors;
using UnityEngine;

[RequireComponent(typeof(UnitAIBrain))]
public class SpawnableUnit : Spawnable
{
    [SerializeField] private UnitAIBrain unitBrain;

    public UnitAIBrain UnitBrain
    {
        get
        {
            if (unitBrain == null)
            {
                unitBrain = GetComponent<UnitAIBrain>();
            }

            return unitBrain;
        }
    }

    private void Reset()
    {
        unitBrain = GetComponent<UnitAIBrain>();
    }

    private void Awake()
    {
        if (unitBrain == null)
        {
            unitBrain = GetComponent<UnitAIBrain>();
        }
    }
}
