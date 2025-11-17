using UnityEngine;
using System.Collections.Generic;


public class PlayerDirtSystem : MonoBehaviour
{
    [Header("Body Parts")]
    [SerializeField] private List<BodyPart> bodyParts = new List<BodyPart>();

    [Header("Materials")]
    [SerializeField] private Material dirtyMaterial; 

    [Header("Dirt Blend Settings")]
    [SerializeField]
    private float targetMetallic = 0.8f;
    [SerializeField]
    private float targetSmoothness = 0.9f;
    [SerializeField]
    private Color dirtTint = new Color(0.05f, 0.1f, 0.05f, 0f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private int _dirtyPartsCount = 0;

    public System.Action<int, int> OnDirtChanged;
    public System.Action OnBecameDirty;
    public System.Action OnBecameClean;
    public System.Action<BodyPart> OnBodyPartDirtied;
    public System.Action<BodyPart> OnBodyPartCleaned;

    public int DirtyPartsCount => _dirtyPartsCount;
    public int TotalPartsCount => bodyParts.Count;
    public bool IsAnyDirty => _dirtyPartsCount > 0;
    public bool IsFullyClean => _dirtyPartsCount == 0;
    public bool IsFullyDirty => _dirtyPartsCount == bodyParts.Count;

  
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int MetallicID = Shader.PropertyToID("_Metallic");
    private static readonly int SmoothID = Shader.PropertyToID("_Smoothness");

    void Start()
    {
        InitializeBodyParts();
        if (enableDebugLogs)
            Debug.Log($"[PlayerDirtSystem] Initialized with {bodyParts.Count} body parts");
    }

    private void InitializeBodyParts()
    {
        foreach (var part in bodyParts)
        {
            if (!part.renderer) continue;

          
            if (part.changeAllMaterials)
                part.originalMaterials = part.renderer.materials;
            else if (part.materialIndex >= 0 && part.materialIndex < part.renderer.materials.Length)
                part.originalMaterial = part.renderer.materials[part.materialIndex];

           
            Material srcMat = null;
            var shared = part.renderer.sharedMaterials;
            if (shared != null && shared.Length > 0)
            {
                int idx = Mathf.Clamp(part.materialIndex, 0, shared.Length - 1);
                srcMat = shared[idx];
            }

            if (srcMat)
            {
                if (srcMat.HasProperty(BaseColorID))
                    part.baseColor = srcMat.GetColor(BaseColorID);
                else if (srcMat.HasProperty(ColorID))
                    part.baseColor = srcMat.GetColor(ColorID);

                if (srcMat.HasProperty(MetallicID))
                    part.baseMetallic = srcMat.GetFloat(MetallicID);

                if (srcMat.HasProperty(SmoothID))
                    part.baseSmoothness = srcMat.GetFloat(SmoothID);
            }

            part.mpb = new MaterialPropertyBlock();
            part.dirtLevel = 0f;
            part.isDirty = false;
        }

        _dirtyPartsCount = 0;
    }

 

    public void AddDirtToAll(float delta)
    {
        bool wasClean = IsFullyClean;
        foreach (var part in bodyParts)
            IncreaseDirtLevel(part, delta);

        if (wasClean && !IsFullyClean) OnBecameDirty?.Invoke();
    }

    public void AddDirtToRandom(int count, float delta)
    {
        var notFull = bodyParts.FindAll(p => p.dirtLevel < 1f);
        if (notFull.Count == 0) return;

        count = Mathf.Clamp(count, 1, notFull.Count);
        for (int i = 0; i < count; i++)
        {
            var p = notFull[Random.Range(0, notFull.Count)];
            IncreaseDirtLevel(p, delta);
        }
    }

    private void IncreaseDirtLevel(BodyPart part, float delta)
    {
        if (!part?.renderer) return;

        float before = part.dirtLevel;
        part.dirtLevel = Mathf.Clamp01(part.dirtLevel + delta);

     
        if (before <= 0f && part.dirtLevel > 0f)
        {
            part.isDirty = true;
            _dirtyPartsCount++;
            OnBodyPartDirtied?.Invoke(part);
            OnDirtChanged?.Invoke(_dirtyPartsCount, bodyParts.Count);
        }

        ApplyDirtBlend(part);

       
    }

    private void ApplyDirtBlend(BodyPart part)
    {
      
        Color target = Color.Lerp(part.baseColor, Color.black + dirtTint, part.dirtLevel);
        float metallic = Mathf.Lerp(part.baseMetallic, targetMetallic, part.dirtLevel);
        float smoothness = Mathf.Lerp(part.baseSmoothness, targetSmoothness, part.dirtLevel);

     
        if (part.changeAllMaterials)
        {
            int subCount = part.renderer.sharedMaterials?.Length ?? 1;
            for (int i = 0; i < subCount; i++)
            {
                part.mpb.Clear();
                if (part.renderer.sharedMaterials[i].HasProperty(BaseColorID))
                    part.mpb.SetColor(BaseColorID, target);
                else
                    part.mpb.SetColor(ColorID, target);

                if (part.renderer.sharedMaterials[i].HasProperty(MetallicID))
                    part.mpb.SetFloat(MetallicID, metallic);
                if (part.renderer.sharedMaterials[i].HasProperty(SmoothID))
                    part.mpb.SetFloat(SmoothID, smoothness);

                part.renderer.SetPropertyBlock(part.mpb, i);
            }
        }
        else
        {
            int i = Mathf.Clamp(part.materialIndex, 0, (part.renderer.sharedMaterials?.Length ?? 1) - 1);
            part.mpb.Clear();
            var mat = part.renderer.sharedMaterials[i];

            if (mat.HasProperty(BaseColorID))
                part.mpb.SetColor(BaseColorID, target);
            else
                part.mpb.SetColor(ColorID, target);

            if (mat.HasProperty(MetallicID))
                part.mpb.SetFloat(MetallicID, metallic);
            if (mat.HasProperty(SmoothID))
                part.mpb.SetFloat(SmoothID, smoothness);

            part.renderer.SetPropertyBlock(part.mpb, i);
        }
    }

  
    public void RemoveDirtFromAll(float delta)
    {
        bool wasAnyDirty = IsAnyDirty;
        foreach (var part in bodyParts)
            DecreaseDirtLevel(part, delta);

        if (wasAnyDirty && IsFullyClean)
            OnBecameClean?.Invoke();
    }

 
    public void RemoveDirtFromRandom(int count, float delta)
    {
        var dirtyParts = bodyParts.FindAll(p => p.dirtLevel > 0f);
        if (dirtyParts.Count == 0) return;

        count = Mathf.Clamp(count, 1, dirtyParts.Count);
        for (int i = 0; i < count; i++)
        {
            var p = dirtyParts[Random.Range(0, dirtyParts.Count)];
            DecreaseDirtLevel(p, delta);
        }
    }


    private void DecreaseDirtLevel(BodyPart part, float delta)
    {
        if (!part?.renderer) return;

        float before = part.dirtLevel;
        part.dirtLevel = Mathf.Clamp01(part.dirtLevel - delta);

       
        if (before > 0f && part.dirtLevel <= 0f)
        {
            if (part.isDirty)
            {
                part.isDirty = false;
                _dirtyPartsCount = Mathf.Max(0, _dirtyPartsCount - 1);
                OnBodyPartCleaned?.Invoke(part);
                OnDirtChanged?.Invoke(_dirtyPartsCount, bodyParts.Count);
            }
        }

       
        ApplyDirtBlend(part);
    }


    public bool DirtyBodyPart(string partName)
    {
        var part = bodyParts.Find(p => p.partName == partName);
        if (part != null && !part.isDirty)
        {
            return DirtyBodyPart(part);
        }
        return false;
    }


    public bool DirtyBodyPart(BodyPart part)
    {
        if (part == null || part.isDirty || part.renderer == null) return false;

        bool wasClean = IsFullyClean;

    
        if (part.changeAllMaterials)
        {
           
            Material[] materials = part.renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = dirtyMaterial;
            }
            part.renderer.materials = materials;
        }
        else
        {
            
            Material[] materials = part.renderer.materials;
            if (part.materialIndex >= 0 && part.materialIndex < materials.Length)
            {
                materials[part.materialIndex] = dirtyMaterial;
                part.renderer.materials = materials;
            }
        }

        part.isDirty = true;
        _dirtyPartsCount++;

        OnBodyPartDirtied?.Invoke(part);
        OnDirtChanged?.Invoke(_dirtyPartsCount, bodyParts.Count);

       
        if (wasClean)
        {
            OnBecameDirty?.Invoke();

            if (enableDebugLogs)
            {
                Debug.Log("[PlayerDirtSystem] Player became dirty!");
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerDirtSystem] {part.partName} became dirty ({_dirtyPartsCount}/{bodyParts.Count})");
        }

        return true;
    }

    public bool CleanRandomDirtyPart()
    {
        var dirtyParts = bodyParts.FindAll(p => p.isDirty);
        if (dirtyParts.Count == 0) return false;

        var randomPart = dirtyParts[Random.Range(0, dirtyParts.Count)];
        return CleanBodyPart(randomPart);
    }

    public bool CleanBodyPart(string partName)
    {
        var part = bodyParts.Find(p => p.partName == partName);
        if (part != null && part.isDirty)
        {
            return CleanBodyPart(part);
        }
        return false;
    }


    public bool CleanBodyPart(BodyPart part)
    {
        if (part == null || !part.isDirty || part.renderer == null) return false;

      
        if (part.changeAllMaterials)
        {
           
            if (part.originalMaterials != null && part.originalMaterials.Length > 0)
            {
                part.renderer.materials = part.originalMaterials;
            }
        }
        else
        {
           
            Material[] materials = part.renderer.materials;
            if (part.materialIndex >= 0 && part.materialIndex < materials.Length)
            {
                materials[part.materialIndex] = part.originalMaterial;
                part.renderer.materials = materials;
            }
        }

        part.isDirty = false;
        _dirtyPartsCount--;

        OnBodyPartCleaned?.Invoke(part);
        OnDirtChanged?.Invoke(_dirtyPartsCount, bodyParts.Count);

       
        if (IsFullyClean)
        {
            OnBecameClean?.Invoke();

            if (enableDebugLogs)
            {
                Debug.Log("[PlayerDirtSystem] Player became fully clean!");
            }
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerDirtSystem] {part.partName} became clean ({_dirtyPartsCount}/{bodyParts.Count})");
        }

        return true;
    }

 
    public void CleanAllBodyParts()
    {
        foreach (var part in bodyParts)
        {
            if (part.isDirty)
            {
                CleanBodyPart(part);
            }
        }
    }

  
    public void DirtyAllBodyParts()
    {
        foreach (var part in bodyParts)
        {
            if (!part.isDirty)
            {
                DirtyBodyPart(part);
            }
        }
    }

   
    public void DirtyRandomParts(int count)
    {
        var cleanParts = bodyParts.FindAll(p => !p.isDirty);
        count = Mathf.Min(count, cleanParts.Count);

        for (int i = 0; i < count; i++)
        {
            if (cleanParts.Count == 0) break;

            int randomIndex = Random.Range(0, cleanParts.Count);
            DirtyBodyPart(cleanParts[randomIndex]);
            cleanParts.RemoveAt(randomIndex);
        }
    }

 
    [ContextMenu("Test: Dirty Random Part")]
    public void TestDirtyRandomPart()
    {
        var cleanParts = bodyParts.FindAll(p => !p.isDirty);
        if (cleanParts.Count > 0)
        {
            DirtyBodyPart(cleanParts[Random.Range(0, cleanParts.Count)]);
        }
    }

    [ContextMenu("Test: Clean Random Part")]
    public void TestCleanRandomPart()
    {
        CleanRandomDirtyPart();
    }

    [ContextMenu("Test: Dirty All")]
    public void TestDirtyAll()
    {
        DirtyAllBodyParts();
    }

    [ContextMenu("Test: Clean All")]
    public void TestCleanAll()
    {
        CleanAllBodyParts();
    }
}

