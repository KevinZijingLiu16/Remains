using UnityEngine;

[System.Serializable]
public class BodyPart
{
    public string partName = "Body Part";
    public Renderer renderer;
   
    public bool changeAllMaterials = false;
   //put indix 0 when changeAllMaterial = false
    public int materialIndex = 0;

    [HideInInspector] public Material originalMaterial;
    [HideInInspector] public Material[] originalMaterials; // 保存所有原始材质
    [HideInInspector] public bool isDirty = false;
}