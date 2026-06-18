using System;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenController : MonoService<UIScreenController>
{
    [Serializable]
    private class UIScreenEntry
    {
        public string key;
        public GameObject screen;
    }

    [SerializeField] private List<UIScreenEntry> screens = new List<UIScreenEntry>();

    private readonly Dictionary<string, GameObject> screensByKey = new Dictionary<string, GameObject>();
    public UISelectionContext CurrentSelectionContext { get; private set; }
    
    private void Awake()
    {
        BuildScreenDictionary();
        RegisterService();
    }

    private void OnDestroy()
    {
        UnregisterService();
    }

    public bool TryGetScreen(string key, out GameObject screen)
    {
        return screensByKey.TryGetValue(key, out screen);
    }

    public GameObject GetScreen(string key)
    {
        return screensByKey.TryGetValue(key, out var screen) ? screen : null;
    }

    public void ShowScreen(string key)
    {
        if (screensByKey.TryGetValue(key, out var screen))
        {
            screen.SetActive(true);
            SendSelectionContext(screen, CurrentSelectionContext);
        }
    }

    public void ShowScreen(string key, UISelectionContext context)
    {
        SetSelectionContext(context);
        ShowScreen(key);
    }

    public void HideScreen(string key)
    {
        if (screensByKey.TryGetValue(key, out var screen))
        {
            screen.SetActive(false);
        }
    }

    public void SetSelectionContext(UISelectionContext context)
    {
        CurrentSelectionContext = context;

        foreach (var screen in screensByKey.Values)
        {
            SendSelectionContext(screen, context);
        }
    }

    private void BuildScreenDictionary()
    {
        screensByKey.Clear();

        foreach (var entry in screens)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.screen == null)
            {
                continue;
            }

            screensByKey[entry.key] = entry.screen;
        }
    }

    private void SendSelectionContext(GameObject screen, UISelectionContext context)
    {
        if (screen == null)
        {
            return;
        }

        var behaviours = screen.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IUIScreenContextReceiver receiver)
            {
                receiver.SetSelectionContext(context);
            }
        }
    }
}
