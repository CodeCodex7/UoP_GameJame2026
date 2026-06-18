using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic Service Locator Pattern
/// </summary>
public static class Services
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();
    private static Dictionary<Type, Action> StoredActions = new Dictionary<Type, Action>();

    public static T Resolve<T>() where T : class
    {
        Type typeParameterType = typeof(T);

        if (services.ContainsKey(typeParameterType))
        {
            return (T)services[typeParameterType];
        }

        Debug.LogError(string.Format("Cant Resovle service of type {0}", typeParameterType));
        return null;
    }

    public static bool TryResolve<T>(out T service) where T : class
    {
        Type typeParameterType = typeof(T);

        if (services.ContainsKey(typeParameterType))
        {
            service = (T)services[typeParameterType];
            return true;
        }

        service = null;
        return false;
    }

    public static bool Registar<T>(object o)
    {
        Type typeParameterType = typeof(T);

        if (services.ContainsKey(typeParameterType))
        {
            Debug.LogError(string.Format("Service {0} all ready registated, Cannot have duplicated services", typeParameterType));
            return false;
        }

        services.Add(typeParameterType, o);

        try
        {
            if (StoredActions.ContainsKey(typeParameterType))
            {
                CallAction(typeParameterType);
            }
        }
        catch
        {
        }

        Debug.Log(string.Format("Service {0} registered successfully", typeParameterType));
        return true;
    }

    public static void UnRegistar<T>()
    {
        Type typeParameterType = typeof(T);

        if (services.ContainsKey(typeParameterType))
        {
            services.Remove(typeParameterType);
            Debug.Log(string.Format("Removed {0} from Service List", typeParameterType));
        }
    }

    public static void ResolveWhenValid<T>(Action action)
    {
        Type typeParameterType = typeof(T);

        if (services.ContainsKey(typeParameterType))
        {
            action();
        }
        else if (StoredActions.ContainsKey(typeParameterType))
        {
            StoredActions[typeParameterType] += action;
        }
        else
        {
            StoredActions.Add(typeParameterType, action);
        }
    }

    private static void CallAction(Type typeParameterType)
    {
        StoredActions[typeParameterType].Invoke();
        StoredActions.Remove(typeParameterType);
    }
}
