using UnityEngine;

public class MonoService<T> : MonoBehaviour
{
    private bool m_Duplicate;

    public void RegisterService()
    {
        if (!Services.Registar<T>(this))
        {
            m_Duplicate = true;
            DestroyImmediate(this);
        }
    }

    public void UnregisterService()
    {
        if (!m_Duplicate)
        {
            Services.UnRegistar<T>();
        }
    }
}
