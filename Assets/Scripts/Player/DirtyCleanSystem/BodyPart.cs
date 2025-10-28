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

    // 新增：连续脏污值(0~1) + 渲染块
    [HideInInspector] public float dirtLevel = 0f;
    [HideInInspector] public MaterialPropertyBlock mpb;

    // 记录原始参数以便混合
    [HideInInspector] public Color baseColor = Color.white;
    [HideInInspector] public float baseMetallic = 0f;
    [HideInInspector] public float baseSmoothness = 0.5f;
}
