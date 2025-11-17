using UnityEngine;

[System.Serializable]
public class BodyPart
{
    public string partName = "Body Part";
    public Renderer renderer;

    public bool changeAllMaterials = false;
    public int materialIndex = 0;

    [HideInInspector] public Material originalMaterial;
    [HideInInspector] public Material[] originalMaterials;

    [HideInInspector] public bool isDirty = false;

 
    [HideInInspector] public float dirtLevel = 0f;
    [HideInInspector] public MaterialPropertyBlock mpb;

 
    [HideInInspector] public Color baseColor = Color.white;
    [HideInInspector] public float baseMetallic = 0f;
    [HideInInspector] public float baseSmoothness = 0.5f;
}
