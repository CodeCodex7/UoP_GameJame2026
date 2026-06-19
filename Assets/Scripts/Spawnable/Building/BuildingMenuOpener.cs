using UnityEngine;

public class BuildingMenuOpener : MonoBehaviour
{
    [SerializeField] private RectTransform m_buttonUI;

    private BuildingMenuUI m_menuReference;

    private void Start()
    {
        m_menuReference = Services.Resolve<BuildingMenuUI>();
    }

    public void OnClick()
    {
        m_menuReference.Initialise(Services.Resolve<SpawnManager>().GetSpawnForType(SpawnableType.Building), OnMenuUICloseCallback);
        m_buttonUI.gameObject.SetActive(false);
    }

    public void OnMenuUICloseCallback()
    {
        m_buttonUI.gameObject.SetActive(true);
    }
}