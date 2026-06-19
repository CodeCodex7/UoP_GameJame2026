using UnityEngine;

public interface ISpawnable
{
    ResourceCost Cost { get; }
    Texture2D UIImage { get; }
    string Description { get; }
    string DisplayName { get; }
    MeshRenderer[] GhostRenderers { get; }
    GameObject GameObject { get; }
    Transform Transform { get; }
}

public class Spawnable : MonoBehaviour, ISpawnable
{
    public ResourceCost Cost;
    public Texture2D UIImage;
    public string Description;
    public string DisplayName;
    public MeshRenderer[] GhostRenderers;

    ResourceCost ISpawnable.Cost => Cost;
    Texture2D ISpawnable.UIImage => UIImage;
    string ISpawnable.Description => Description;
    string ISpawnable.DisplayName => DisplayName;
    MeshRenderer[] ISpawnable.GhostRenderers => GhostRenderers;
    GameObject ISpawnable.GameObject => gameObject;
    Transform ISpawnable.Transform => transform;
}
