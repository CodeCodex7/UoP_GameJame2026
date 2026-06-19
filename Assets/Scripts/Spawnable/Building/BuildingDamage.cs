using AI.Goap.UnitAI.Behaviors;
using UnityEngine;
using UnityEngine.Events;

public class BuildingDamage : MonoBehaviour, IDamage, ITeam
{
    [SerializeField] private string teamId = "Enemy";
    [SerializeField] private int maxHp = 200;
    [SerializeField] private int currentHp;
    [SerializeField] private bool deleteOnDeath = true;
    [SerializeField] private UnityEvent onDeath;

    public Transform Transform => transform;
    public string TeamId => teamId;
    public int MaxHp => maxHp;
    public int CurrentHp => currentHp;
    public bool IsAlive => currentHp > 0;

    private void Awake()
    {
        if (currentHp <= 0)
        {
            currentHp = maxHp;
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        currentHp = Mathf.Max(0, currentHp - amount);

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || !IsAlive)
        {
            return;
        }

        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    private void Die()
    {
        onDeath?.Invoke();

        if (deleteOnDeath)
        {
            Destroy(gameObject);
        }
    }
}
