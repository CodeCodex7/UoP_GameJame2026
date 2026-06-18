using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SpawnableObjectMenuEntry : MonoBehaviour
{
    private RawImage m_displayPicture;
    private Text m_metalCost;
    private Text m_woodCost;
    private Text m_mushroomCost;
    
    
    private Spawnable spawnable;

    public void Init(Spawnable spawnable)
    {
        this.spawnable = spawnable;

        m_displayPicture.texture = spawnable.UIImage;
        m_metalCost.text = spawnable.Cost.metal.ToString();
        m_woodCost.text = spawnable.Cost.wood.ToString();
        m_mushroomCost.text = spawnable.Cost.mushrooms.ToString();
        
    }
}