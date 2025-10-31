using UnityEngine;
using UnityEngine.Video;

public class EnhancedManagedInteractionTrigger : MonoBehaviour
{
    [Header("提示类型")]
    public EnhancedInteractionPromptManager.PromptType promptType = EnhancedInteractionPromptManager.PromptType.Text;

    [Header("文本设置")]
    [TextArea(2, 4)]
    public string promptText = "Press Tab To Interact";

    [Header("图片设置")]
    public Sprite promptImage;
    public Sprite iconImage; // 用于文字旁边的小图标

    [Header("视频设置")]
    public VideoClip promptVideo;
    public bool loopVideo = false;

   
    public enum LayoutStyle
    {
        Horizontal,  // 水平排列
        Vertical,    // 垂直排列
        Overlay      // 叠加显示
    }
    public LayoutStyle layoutStyle = LayoutStyle.Horizontal;

    [Header("基本设置")]
    [Range(0, 10)]
    public int priority = 1;
    public string playerTag = "Player";
    public bool showOnEnter = true;
    public bool hideOnExit = true;

    [Header("定时显示")]
    public bool useTimedDisplay = false;
    public float displayDuration = 3f;

    [Header("动画效果")]
    public bool useAnimation = false;
    public AnimationCurve showAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float animationDuration = 0.5f;

    [Header("按键检测")]
    public bool enableKeyDetection = false;
    public KeyCode interactionKey = KeyCode.Tab;
    public bool hideAfterInteraction = true;

    [Header("事件回调")]
    public UnityEngine.Events.UnityEvent onPlayerEnter;
    public UnityEngine.Events.UnityEvent onPlayerExit;
    public UnityEngine.Events.UnityEvent onInteractionKeyPressed;
    public UnityEngine.Events.UnityEvent onPromptShown;
    public UnityEngine.Events.UnityEvent onPromptHidden;

    [Header("调试")]
    public bool debugMode = false;

    private bool isPlayerInRange = false;
    private GameObject playerObject;
    private bool isShowingPrompt = false;

    private void Start()
    {
        ValidateSetup();
    }

    private void ValidateSetup()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"EnhancedManagedInteractionTrigger on {gameObject.name}: Missing Collider component!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"EnhancedManagedInteractionTrigger on {gameObject.name}: Collider is not set as Trigger!");
        }

        // 验证提示内容
        switch (promptType)
        {
            case EnhancedInteractionPromptManager.PromptType.Text:
                if (string.IsNullOrEmpty(promptText))
                {
                    Debug.LogWarning($"Text prompt is empty on {gameObject.name}");
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.Image:
                if (promptImage == null)
                {
                    Debug.LogWarning($"Image prompt is null on {gameObject.name}");
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.Video:
                if (promptVideo == null)
                {
                    Debug.LogWarning($"Video prompt is null on {gameObject.name}");
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.TextWithImage:
                if (string.IsNullOrEmpty(promptText) && promptImage == null)
                {
                    Debug.LogWarning($"Both text and image are empty on {gameObject.name}");
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.TextWithVideo:
                if (string.IsNullOrEmpty(promptText) && promptVideo == null)
                {
                    Debug.LogWarning($"Both text and video are empty on {gameObject.name}");
                }
                break;
        }
    }

    private void Update()
    {
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
                Debug.Log($"Player entered: {gameObject.name}");
            }

            if (showOnEnter)
            {
                ShowPrompt();
            }

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
                Debug.Log($"Player exited: {gameObject.name}");
            }

            if (hideOnExit)
            {
                HidePrompt();
            }

            onPlayerExit?.Invoke();
        }
    }

    private void OnInteractionKeyPressed()
    {
        if (debugMode)
        {
            Debug.Log($"Interaction key pressed: {gameObject.name}");
        }

        onInteractionKeyPressed?.Invoke();

        if (hideAfterInteraction)
        {
            HidePrompt();
        }
    }

    public void ShowPrompt()
    {
        if (EnhancedInteractionPromptManager.Instance == null)
        {
            Debug.LogWarning("EnhancedInteractionPromptManager.Instance is null!");
            return;
        }

        switch (promptType)
        {
            case EnhancedInteractionPromptManager.PromptType.Text:
                if (useTimedDisplay)
                {
                    EnhancedInteractionPromptManager.Instance.ShowPromptTimed(promptText, displayDuration, priority);
                }
                else
                {
                    EnhancedInteractionPromptManager.Instance.ShowTextPrompt(promptText, priority);
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.Image:
                if (promptImage != null)
                {
                    if (useTimedDisplay)
                    {
                        EnhancedInteractionPromptManager.Instance.ShowImagePromptTimed(promptImage, displayDuration, priority);
                    }
                    else
                    {
                        EnhancedInteractionPromptManager.Instance.ShowImagePrompt(promptImage, priority);
                    }
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.Video:
                if (promptVideo != null)
                {
                    EnhancedInteractionPromptManager.Instance.ShowVideoPrompt(promptVideo, priority, loopVideo);
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.TextWithImage:
                if (iconImage != null)
                {
                    EnhancedInteractionPromptManager.Instance.ShowPromptWithIcon(promptText, iconImage, priority);
                }
                else
                {
                    EnhancedInteractionPromptManager.Instance.ShowTextWithImagePrompt(promptText, promptImage, priority);
                }
                break;

            case EnhancedInteractionPromptManager.PromptType.TextWithVideo:
                if (promptVideo != null)
                {
                    EnhancedInteractionPromptManager.Instance.ShowTextWithVideoPrompt(promptText, promptVideo, priority, loopVideo);
                }
                break;
        }

        isShowingPrompt = true;
        onPromptShown?.Invoke();

        if (debugMode)
        {
            Debug.Log($"Showing {promptType} prompt: {gameObject.name}");
        }
    }

    public void HidePrompt()
    {
        if (EnhancedInteractionPromptManager.Instance != null)
        {
            EnhancedInteractionPromptManager.Instance.HidePrompt(priority);
        }

        isShowingPrompt = false;
        onPromptHidden?.Invoke();

        if (debugMode)
        {
            Debug.Log($"Hiding prompt: {gameObject.name}");
        }
    }

    // 动态更新提示内容
    public void UpdatePromptText(string newText)
    {
        promptText = newText;
        if (isShowingPrompt && isPlayerInRange)
        {
            ShowPrompt();
        }
    }

    public void UpdatePromptImage(Sprite newImage)
    {
        promptImage = newImage;
        if (isShowingPrompt && isPlayerInRange)
        {
            ShowPrompt();
        }
    }

    public void UpdatePromptVideo(VideoClip newVideo)
    {
        promptVideo = newVideo;
        if (isShowingPrompt && isPlayerInRange)
        {
            ShowPrompt();
        }
    }

    public void ChangePromptType(EnhancedInteractionPromptManager.PromptType newType)
    {
        promptType = newType;
        if (isShowingPrompt && isPlayerInRange)
        {
            ShowPrompt();
        }
    }

    // 获取当前状态
    public bool IsPlayerInRange() => isPlayerInRange;
    public GameObject GetPlayerObject() => playerObject;
    public bool IsShowingPrompt() => isShowingPrompt;

    // Gizmos绘制
    private void OnDrawGizmos()
    {
        Gizmos.color = isPlayerInRange ? Color.green : Color.yellow;

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            DrawColliderGizmo(col, false);
        }

        // 绘制提示类型图标
        if (Application.isEditor)
        {
            Vector3 iconPos = transform.position + Vector3.up * 2f;

            switch (promptType)
            {
                case EnhancedInteractionPromptManager.PromptType.Text:
                    Gizmos.color = Color.white;
                    break;
                case EnhancedInteractionPromptManager.PromptType.Image:
                    Gizmos.color = Color.cyan;
                    break;
                case EnhancedInteractionPromptManager.PromptType.Video:
                    Gizmos.color = Color.red;
                    break;
                case EnhancedInteractionPromptManager.PromptType.TextWithImage:
                    Gizmos.color = Color.blue;
                    break;
                case EnhancedInteractionPromptManager.PromptType.TextWithVideo:
                    Gizmos.color = Color.magenta;
                    break;
            }

            Gizmos.DrawWireSphere(iconPos, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            DrawColliderGizmo(col, true);
        }
    }

    private void DrawColliderGizmo(Collider col, bool filled)
    {
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (filled)
                Gizmos.DrawCube(box.center, box.size);
            else
                Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphere = col as SphereCollider;
            Vector3 center = transform.position + sphere.center;

            if (filled)
            {
                // Unity doesn't have DrawSphere, so we use multiple wire spheres
                for (int i = 0; i < 3; i++)
                {
                    Gizmos.DrawWireSphere(center, sphere.radius * transform.lossyScale.x * (1f - i * 0.3f));
                }
            }
            else
            {
                Gizmos.DrawWireSphere(center, sphere.radius * transform.lossyScale.x);
            }
        }
        else if (col is CapsuleCollider)
        {
            CapsuleCollider capsule = col as CapsuleCollider;
            Vector3 center = transform.position + capsule.center;

            // 简化的胶囊体绘制
            Gizmos.DrawWireSphere(center + Vector3.up * (capsule.height * 0.5f - capsule.radius), capsule.radius);
            Gizmos.DrawWireSphere(center - Vector3.up * (capsule.height * 0.5f - capsule.radius), capsule.radius);
            Gizmos.DrawWireCube(center, new Vector3(capsule.radius * 2, capsule.height - capsule.radius * 2, capsule.radius * 2));
        }
    }
}