using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class InteractionPrompt : MonoBehaviour
{

 
    [Tooltip("提示文本内容")]
    [TextArea(2, 4)]
    public string promptText = "Press Tab To Equip Tool";

    [Tooltip("玩家的Tag")]
    public string playerTag = "Player";

    [Header("UI引用")]
    [Tooltip("TextMeshPro UI组件引用")]
    public TextMeshProUGUI promptUI;

    [Header("按键检测（可选）")]
    [Tooltip("是否启用按键检测")]
    public bool enableKeyDetection = false;

    [Tooltip("要检测的按键")]
    public KeyCode interactionKey = KeyCode.Tab;

    [Tooltip("按键按下后是否隐藏提示")]
    public bool hidePromptAfterKeyPress = true;

    [Header("事件回调")]
    [Tooltip("玩家进入触发区域时触发")]
    public UnityEvent onPlayerEnter;

    [Tooltip("玩家离开触发区域时触发")]
    public UnityEvent onPlayerExit;

    [Tooltip("按下交互键时触发")]
    public UnityEvent onInteractionKeyPressed;

    [Header("显示设置")]
    [Tooltip("是否在进入时自动显示")]
    public bool showOnEnter = true;

    [Tooltip("是否在离开时自动隐藏")]
    public bool hideOnExit = true;

    [Tooltip("淡入淡出速度")]
    public float fadeSpeed = 5f;

    [Tooltip("是否使用淡入淡出效果")]
    public bool useFadeEffect = true;

    [Header("调试")]
    [Tooltip("是否在Console输出调试信息")]
    public bool debugMode = false;

    private bool isPlayerInRange = false;
    private CanvasGroup canvasGroup;
    private float targetAlpha = 0f;
    private GameObject playerObject;
    private string currentDisplayedText = "";

    private void Awake()
    {
        // 在Awake中初始化UI，确保在Start之前完成
        InitializeUI();
    }

    private void Start()
    {
        ValidateSetup();

        // 确保文本正确设置
        UpdatePromptTextImmediate(promptText);
    }

    private void InitializeUI()
    {
        if (promptUI != null)
        {
            // 获取或添加CanvasGroup用于淡入淡出
            canvasGroup = promptUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null && useFadeEffect)
            {
                canvasGroup = promptUI.gameObject.AddComponent<CanvasGroup>();
            }

            // 初始隐藏
            if (useFadeEffect && canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                promptUI.gameObject.SetActive(true); // 保持激活但透明度为0
            }
            else
            {
                promptUI.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"InteractionPromptFixed on {gameObject.name}: 没有分配 TextMeshProUGUI 组件！");
        }
    }

    private void ValidateSetup()
    {
        // 检查Collider设置
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"InteractionPromptFixed on {gameObject.name}: 没有Collider组件！请添加Collider。");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"InteractionPromptFixed on {gameObject.name}: Collider 没有设置为 Trigger！");
        }
    }

    private void Update()
    {
        // 处理淡入淡出效果
        if (useFadeEffect && canvasGroup != null && promptUI != null)
        {
            float previousAlpha = canvasGroup.alpha;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

            // 当完全隐藏时，禁用GameObject以优化性能
            if (canvasGroup.alpha < 0.01f && targetAlpha < 0.5f && promptUI.gameObject.activeSelf)
            {
                if (!useFadeEffect)
                {
                    promptUI.gameObject.SetActive(false);
                }
            }
        }

        // 检测按键输入
        if (enableKeyDetection && isPlayerInRange && Input.GetKeyDown(interactionKey))
        {
            OnInteractionKeyPressed();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            playerObject = other.gameObject;

            if (debugMode)
            {
                Debug.Log($"Player entered interaction zone: {gameObject.name}, Text: {promptText}");
            }

            // 确保文本是最新的
            UpdatePromptTextImmediate(promptText);

            if (showOnEnter)
            {
                ShowPrompt();
            }

            // 触发进入事件
            onPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            playerObject = null;

            if (debugMode)
            {
                Debug.Log($"Player exited interaction zone: {gameObject.name}");
            }

            if (hideOnExit)
            {
                HidePrompt();
            }

            // 触发离开事件
            onPlayerExit?.Invoke();
        }
    }

    private void OnInteractionKeyPressed()
    {
        if (debugMode)
        {
            Debug.Log($"Interaction key pressed in zone: {gameObject.name}");
        }

        // 触发按键事件
        onInteractionKeyPressed?.Invoke();

        // 可选：按键后隐藏提示
        if (hidePromptAfterKeyPress)
        {
            HidePrompt();
        }
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    public void ShowPrompt()
    {
        if (promptUI == null) return;

        // 确保显示正确的文本
        if (currentDisplayedText != promptText)
        {
            UpdatePromptTextImmediate(promptText);
        }

        if (useFadeEffect && canvasGroup != null)
        {
            promptUI.gameObject.SetActive(true);
            targetAlpha = 1f;
        }
        else
        {
            promptUI.gameObject.SetActive(true);
        }

        if (debugMode)
        {
            Debug.Log($"Showing prompt: {promptText}");
        }
    }

    /// <summary>
    /// 隐藏提示
    /// </summary>
    public void HidePrompt()
    {
        if (promptUI == null) return;

        if (useFadeEffect && canvasGroup != null)
        {
            targetAlpha = 0f;
        }
        else
        {
            promptUI.gameObject.SetActive(false);
        }

        if (debugMode)
        {
            Debug.Log($"Hiding prompt: {promptText}");
        }
    }

    /// <summary>
    /// 立即更新提示文本（不经过淡入淡出）
    /// </summary>
    private void UpdatePromptTextImmediate(string newText)
    {
        if (promptUI != null && currentDisplayedText != newText)
        {
            promptUI.text = newText;
            currentDisplayedText = newText;

            if (debugMode)
            {
                Debug.Log($"Updated prompt text to: {newText}");
            }
        }
    }

    /// <summary>
    /// 更新提示文本（公共方法）
    /// </summary>
    public void UpdatePromptText(string newText)
    {
        promptText = newText;
        UpdatePromptTextImmediate(newText);
    }

    /// <summary>
    /// 检查玩家是否在范围内
    /// </summary>
    public bool IsPlayerInRange()
    {
        return isPlayerInRange;
    }

    /// <summary>
    /// 获取范围内的玩家对象
    /// </summary>
    public GameObject GetPlayerInRange()
    {
        return playerObject;
    }

    /// <summary>
    /// 设置是否启用按键检测
    /// </summary>
    public void SetKeyDetectionEnabled(bool enabled)
    {
        enableKeyDetection = enabled;
    }

    /// <summary>
    /// 改变交互按键
    /// </summary>
    public void SetInteractionKey(KeyCode newKey)
    {
        interactionKey = newKey;
    }

    /// <summary>
    /// 强制刷新UI显示
    /// </summary>
    public void ForceRefreshUI()
    {
        if (promptUI != null)
        {
            UpdatePromptTextImmediate(promptText);
            if (debugMode)
            {
                Debug.Log($"Force refreshed UI with text: {promptText}");
            }
        }
    }

    // Inspector值改变时调用
    private void OnValidate()
    {
        // 在编辑器中改变值时，立即更新UI（仅在Play模式下）
        if (Application.isPlaying && promptUI != null)
        {
            UpdatePromptTextImmediate(promptText);
        }
    }

    // 在Scene视图中显示触发区域
    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsule = col as CapsuleCollider;
                Gizmos.DrawWireSphere(transform.position + capsule.center, capsule.radius);
            }
        }
    }

    // 在Inspector中显示帮助信息
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        Collider col = GetComponent<Collider>();
        if (col != null && col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}