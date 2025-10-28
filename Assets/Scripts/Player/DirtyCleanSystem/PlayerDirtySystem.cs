using UnityEngine;
using System.Collections.Generic;


public class PlayerDirtSystem : MonoBehaviour
{
    [Header("Body Parts")]
    [SerializeField] private List<BodyPart> bodyParts = new List<BodyPart>();

    [Header("Materials")]
    [SerializeField] private Material dirtyMaterial; // 兼容你的旧接口

    [Header("Dirt Blend Settings")]
    [SerializeField, Tooltip("脏污目标的金属度(0~1)")]
    private float targetMetallic = 0.8f;
    [SerializeField, Tooltip("脏污目标的光滑度(0~1)")]
    private float targetSmoothness = 0.9f;
    [SerializeField, Tooltip("变黑时额外的颜色偏移，可保持微弱绿感")]
    private Color dirtTint = new Color(0.05f, 0.1f, 0.05f, 0f);

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private int _dirtyPartsCount = 0;

    // Events（保持不变）...
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

    // 常用的属性ID（URP 使用 _BaseColor，内置管线使用 _Color）
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

            // 兼容你原来的“还原材质”逻辑：
            if (part.changeAllMaterials)
                part.originalMaterials = part.renderer.materials;
            else if (part.materialIndex >= 0 && part.materialIndex < part.renderer.materials.Length)
                part.originalMaterial = part.renderer.materials[part.materialIndex];

            // —— 新增：记录原始参数（读取 sharedMaterials，避免实例化）
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

    // === 新增：连续脏污 API ===================================

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

        // “从0到>0”算作变脏一次，保持你原本事件统计
        if (before <= 0f && part.dirtLevel > 0f)
        {
            part.isDirty = true;
            _dirtyPartsCount++;
            OnBodyPartDirtied?.Invoke(part);
            OnDirtChanged?.Invoke(_dirtyPartsCount, bodyParts.Count);
        }

        ApplyDirtBlend(part);

        // “满黑”不做特殊处理；如果需要可在此触发事件
    }

    private void ApplyDirtBlend(BodyPart part)
    {
        // 颜色：从原色 -> 偏黑 + 轻微绿
        Color target = Color.Lerp(part.baseColor, Color.black + dirtTint, part.dirtLevel);
        float metallic = Mathf.Lerp(part.baseMetallic, targetMetallic, part.dirtLevel);
        float smoothness = Mathf.Lerp(part.baseSmoothness, targetSmoothness, part.dirtLevel);

        // 给指定 submesh 设置 MPB；如需全材质就循环所有 submesh
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

    // 从所有部位减少脏污
    public void RemoveDirtFromAll(float delta)
    {
        bool wasAnyDirty = IsAnyDirty;
        foreach (var part in bodyParts)
            DecreaseDirtLevel(part, delta);

        if (wasAnyDirty && IsFullyClean)
            OnBecameClean?.Invoke();
    }

    // 从若干部位随机减少脏污（只挑“>0”的）
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

    // 内部：降低单个部位的 dirtLevel 并更新显示/事件
    private void DecreaseDirtLevel(BodyPart part, float delta)
    {
        if (!part?.renderer) return;

        float before = part.dirtLevel;
        part.dirtLevel = Mathf.Clamp01(part.dirtLevel - delta);

        // 从“>0”降到“=0”视为清洁完成，触发事件与计数
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

        // 实时刷新外观（与增加脏污时同一个 ApplyDirtBlend）
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

