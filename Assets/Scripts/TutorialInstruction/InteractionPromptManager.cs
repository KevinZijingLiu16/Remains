using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Video;

public class EnhancedInteractionPromptManager : MonoBehaviour
{
    public static EnhancedInteractionPromptManager Instance { get; private set; }

    [Header("UI Components")]
    public TextMeshProUGUI promptTextUI;
    public Image promptImageUI;
    public RawImage videoDisplayUI;
    public VideoPlayer videoPlayer;

    [Header("Layout")]
    public GameObject textContainer;
    public GameObject imageContainer;
    public GameObject videoContainer;
    public GameObject promptPanel; // 整个提示面板

    [Header("Settings")]
    public float fadeSpeed = 5f;
    public bool useFadeEffect = true;
    public bool usePrioritySystem = true;

    [Header("Default Sizes")]
    public Vector2 defaultImageSize = new Vector2(200, 200);
    public Vector2 defaultVideoSize = new Vector2(400, 300);

    // 提示类型枚举
    public enum PromptType
    {
        Text,
        Image,
        Video,
        TextWithImage,
        TextWithVideo
    }

    private CanvasGroup panelCanvasGroup;
    private float targetAlpha = 0f;
    private string currentText = "";
    private Sprite currentImage = null;
    private VideoClip currentVideo = null;
    private int currentPriority = 0;
    private PromptType currentType = PromptType.Text;
    private Coroutine hideCoroutine;
    private RenderTexture videoRenderTexture;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        // 初始化主面板的CanvasGroup
        if (promptPanel != null)
        {
            panelCanvasGroup = promptPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null && useFadeEffect)
            {
                panelCanvasGroup = promptPanel.AddComponent<CanvasGroup>();
            }

            if (useFadeEffect && panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
            }
            else
            {
                promptPanel.SetActive(false);
            }
        }

        // 初始化视频播放器
        if (videoPlayer != null && videoDisplayUI != null)
        {
            // 创建RenderTexture用于视频显示
            videoRenderTexture = new RenderTexture(
                (int)defaultVideoSize.x,
                (int)defaultVideoSize.y,
                16
            );
            videoPlayer.targetTexture = videoRenderTexture;
            videoDisplayUI.texture = videoRenderTexture;

            // 设置视频播放完成回调
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // 初始隐藏所有容器
        HideAllContainers();
    }

    private void Update()
    {
        if (useFadeEffect && panelCanvasGroup != null && promptPanel != null)
        {
            panelCanvasGroup.alpha = Mathf.Lerp(panelCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

            if (panelCanvasGroup.alpha < 0.01f && targetAlpha < 0.5f)
            {
                promptPanel.SetActive(false);
                StopVideo();
            }
        }
    }

    #region 显示不同类型的提示

    // 显示纯文本提示
    public void ShowTextPrompt(string text, int priority = 0)
    {
        if (!CanShowPrompt(priority)) return;

        PrepareNewPrompt(priority);

        currentType = PromptType.Text;
        currentText = text;

        if (promptTextUI != null)
        {
            promptTextUI.text = text;
        }

        ShowContainers(true, false, false);
        ShowPanel();
    }

    // 显示纯图片提示
    public void ShowImagePrompt(Sprite image, int priority = 0)
    {
        if (!CanShowPrompt(priority) || image == null) return;

        PrepareNewPrompt(priority);

        currentType = PromptType.Image;
        currentImage = image;

        if (promptImageUI != null)
        {
            promptImageUI.sprite = image;
            promptImageUI.SetNativeSize();
            FitImageToSize();
        }

        ShowContainers(false, true, false);
        ShowPanel();
    }

    // 显示纯视频提示
    public void ShowVideoPrompt(VideoClip video, int priority = 0, bool loop = false)
    {
        if (!CanShowPrompt(priority) || video == null) return;

        PrepareNewPrompt(priority);

        currentType = PromptType.Video;
        currentVideo = video;

        if (videoPlayer != null)
        {
            videoPlayer.clip = video;
            videoPlayer.isLooping = loop;
            videoPlayer.Play();
        }

        ShowContainers(false, false, true);
        ShowPanel();
    }

    // 显示文本+图片提示
    public void ShowTextWithImagePrompt(string text, Sprite image, int priority = 0)
    {
        if (!CanShowPrompt(priority)) return;

        PrepareNewPrompt(priority);

        currentType = PromptType.TextWithImage;
        currentText = text;
        currentImage = image;

        if (promptTextUI != null)
        {
            promptTextUI.text = text;
        }

        if (promptImageUI != null && image != null)
        {
            promptImageUI.sprite = image;
            FitImageToSize();
        }

        ShowContainers(true, image != null, false);
        ShowPanel();
    }

    // 显示文本+视频提示
    public void ShowTextWithVideoPrompt(string text, VideoClip video, int priority = 0, bool loop = false)
    {
        if (!CanShowPrompt(priority)) return;

        PrepareNewPrompt(priority);

        currentType = PromptType.TextWithVideo;
        currentText = text;
        currentVideo = video;

        if (promptTextUI != null)
        {
            promptTextUI.text = text;
        }

        if (videoPlayer != null && video != null)
        {
            videoPlayer.clip = video;
            videoPlayer.isLooping = loop;
            videoPlayer.Play();
        }

        ShowContainers(true, false, video != null);
        ShowPanel();
    }

    // 显示带图标的文本提示
    public void ShowPromptWithIcon(string text, Sprite icon, int priority = 0)
    {
        ShowTextWithImagePrompt(text, icon, priority);
    }

    #endregion

    #region 隐藏提示

    public void HidePrompt(int priority = 0)
    {
        if (usePrioritySystem && priority < currentPriority)
        {
            return;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        ResetPromptData();

        if (useFadeEffect && panelCanvasGroup != null)
        {
            targetAlpha = 0f;
        }
        else if (promptPanel != null)
        {
            promptPanel.SetActive(false);
            StopVideo();
        }
    }

    public void ShowPromptTimed(string text, float duration, int priority = 0)
    {
        ShowTextPrompt(text, priority);
        hideCoroutine = StartCoroutine(HideAfterDelay(duration, priority));
    }

    public void ShowImagePromptTimed(Sprite image, float duration, int priority = 0)
    {
        ShowImagePrompt(image, priority);
        hideCoroutine = StartCoroutine(HideAfterDelay(duration, priority));
    }

    #endregion

    #region 辅助方法

    private bool CanShowPrompt(int priority)
    {
        if (promptPanel == null) return false;

        if (usePrioritySystem && priority < currentPriority)
        {
            return false;
        }

        return true;
    }

    private void PrepareNewPrompt(int priority)
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        StopVideo();
        currentPriority = priority;
    }

    private void ShowContainers(bool showText, bool showImage, bool showVideo)
    {
        if (textContainer != null)
            textContainer.SetActive(showText);

        if (imageContainer != null)
            imageContainer.SetActive(showImage);

        if (videoContainer != null)
            videoContainer.SetActive(showVideo);
    }

    private void HideAllContainers()
    {
        ShowContainers(false, false, false);
    }

    private void ShowPanel()
    {
        if (promptPanel == null) return;

        if (useFadeEffect && panelCanvasGroup != null)
        {
            promptPanel.SetActive(true);
            targetAlpha = 1f;
        }
        else
        {
            promptPanel.SetActive(true);
        }
    }

    private void FitImageToSize()
    {
        if (promptImageUI == null) return;

        RectTransform rt = promptImageUI.GetComponent<RectTransform>();
        if (rt != null)
        {
            // 保持宽高比适应默认大小
            float imageRatio = promptImageUI.sprite.rect.width / promptImageUI.sprite.rect.height;
            float targetRatio = defaultImageSize.x / defaultImageSize.y;

            if (imageRatio > targetRatio)
            {
                rt.sizeDelta = new Vector2(defaultImageSize.x, defaultImageSize.x / imageRatio);
            }
            else
            {
                rt.sizeDelta = new Vector2(defaultImageSize.y * imageRatio, defaultImageSize.y);
            }
        }
    }

    private void StopVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // 视频播放完成的回调
        if (!vp.isLooping)
        {
            Debug.Log("Video finished playing");
        }
    }

    private void ResetPromptData()
    {
        currentPriority = 0;
        currentText = "";
        currentImage = null;
        currentVideo = null;
        currentType = PromptType.Text;
    }

    private IEnumerator HideAfterDelay(float delay, int priority)
    {
        yield return new WaitForSeconds(delay);
        HidePrompt(priority);
        hideCoroutine = null;
    }

    #endregion

    #region 公共获取方法

    public string GetCurrentText() => currentText;
    public Sprite GetCurrentImage() => currentImage;
    public VideoClip GetCurrentVideo() => currentVideo;
    public int GetCurrentPriority() => currentPriority;
    public PromptType GetCurrentType() => currentType;

    public bool IsShowingPrompt()
    {
        return currentPriority > 0 &&
               (currentType != PromptType.Text || !string.IsNullOrEmpty(currentText));
    }

    #endregion

    private void OnDestroy()
    {
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}