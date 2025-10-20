using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 管理玩家身体部位的脏污状态
/// </summary>
public class PlayerDirtSystem : MonoBehaviour
{
    [Header("Body Parts")]
    [SerializeField] private List<BodyPart> bodyParts = new List<BodyPart>();

    [Header("Materials")]
    [SerializeField] private Material dirtyMaterial;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private int _dirtyPartsCount = 0;

    // Events
    public System.Action<int, int> OnDirtChanged; // (dirtyCount, totalCount)
    public System.Action OnBecameDirty; // 第一次变脏
    public System.Action OnBecameClean; // 完全干净
    public System.Action<BodyPart> OnBodyPartDirtied; // 某个部位变脏
    public System.Action<BodyPart> OnBodyPartCleaned; // 某个部位变干净

    public int DirtyPartsCount => _dirtyPartsCount;
    public int TotalPartsCount => bodyParts.Count;
    public bool IsAnyDirty => _dirtyPartsCount > 0;
    public bool IsFullyClean => _dirtyPartsCount == 0;
    public bool IsFullyDirty => _dirtyPartsCount == bodyParts.Count;

    void Start()
    {
        InitializeBodyParts();

        if (enableDebugLogs)
        {
            Debug.Log($"[PlayerDirtSystem] Initialized with {bodyParts.Count} body parts");
        }
    }

    private void InitializeBodyParts()
    {
        foreach (var part in bodyParts)
        {
            if (part.renderer != null)
            {
                if (part.changeAllMaterials)
                {
                    // 保存所有原始材质
                    part.originalMaterials = part.renderer.materials;
                }
                else if (part.materialIndex >= 0 && part.materialIndex < part.renderer.materials.Length)
                {
                    // 只保存指定索引的材质
                    part.originalMaterial = part.renderer.materials[part.materialIndex];
                }

                part.isDirty = false;
            }
        }

        _dirtyPartsCount = 0;
    }

    /// <summary>
    /// 让指定部位变脏
    /// </summary>
    public bool DirtyBodyPart(string partName)
    {
        var part = bodyParts.Find(p => p.partName == partName);
        if (part != null && !part.isDirty)
        {
            return DirtyBodyPart(part);
        }
        return false;
    }

    /// <summary>
    /// 让指定部位变脏
    /// </summary>
    public bool DirtyBodyPart(BodyPart part)
    {
        if (part == null || part.isDirty || part.renderer == null) return false;

        bool wasClean = IsFullyClean;

        // 改变材质
        if (part.changeAllMaterials)
        {
            // 改变所有材质
            Material[] materials = part.renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = dirtyMaterial;
            }
            part.renderer.materials = materials;
        }
        else
        {
            // 只改变指定索引的材质
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

        // 如果是第一次变脏
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

    /// <summary>
    /// 让随机一个脏的部位变干净
    /// </summary>
    public bool CleanRandomDirtyPart()
    {
        var dirtyParts = bodyParts.FindAll(p => p.isDirty);
        if (dirtyParts.Count == 0) return false;

        var randomPart = dirtyParts[Random.Range(0, dirtyParts.Count)];
        return CleanBodyPart(randomPart);
    }

    /// <summary>
    /// 让指定部位变干净
    /// </summary>
    public bool CleanBodyPart(string partName)
    {
        var part = bodyParts.Find(p => p.partName == partName);
        if (part != null && part.isDirty)
        {
            return CleanBodyPart(part);
        }
        return false;
    }

    /// <summary>
    /// 让指定部位变干净
    /// </summary>
    public bool CleanBodyPart(BodyPart part)
    {
        if (part == null || !part.isDirty || part.renderer == null) return false;

        // 恢复原材质
        if (part.changeAllMaterials)
        {
            // 恢复所有材质
            if (part.originalMaterials != null && part.originalMaterials.Length > 0)
            {
                part.renderer.materials = part.originalMaterials;
            }
        }
        else
        {
            // 只恢复指定索引的材质
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

        // 如果完全干净了
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

    /// <summary>
    /// 让所有部位变干净
    /// </summary>
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

    /// <summary>
    /// 让所有部位变脏
    /// </summary>
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

    /// <summary>
    /// 让随机N个部位变脏
    /// </summary>
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

    // Context Menu for testing
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

