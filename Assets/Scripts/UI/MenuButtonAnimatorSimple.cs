using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class MenuButtonAnimatorSimple : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float animationDuration = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private string hoverSoundName = "UI_Hover";
    [SerializeField] private string clickSoundName = "UI_Click";
    
    private Image _buttonImage;
    private Vector3 _originalScale;
    private Button _button;
    private bool _isHovered = false;
    
    private Coroutine _scaleCoroutine;
    private Coroutine _colorCoroutine;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _buttonImage = GetComponent<Image>();
        
        if (_buttonImage == null)
        {
            _buttonImage = GetComponentInChildren<Image>();
        }
        
        _originalScale = transform.localScale;
        
        if (_buttonImage != null)
        {
            _buttonImage.color = normalColor;
        }
        
        _button.onClick.AddListener(PlayClickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.interactable)
        {
            SetHoverState(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_button.interactable)
        {
            SetHoverState(false);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_button.interactable)
        {
            SetHoverState(true);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_button.interactable)
        {
            SetHoverState(false);
        }
    }

    private void SetHoverState(bool hover)
    {
        if (_isHovered == hover) return;
        
        _isHovered = hover;
        
        if (hover && !string.IsNullOrEmpty(hoverSoundName))
        {
            PlayHoverSound();
        }
        
        Vector3 targetScale = hover ? _originalScale * hoverScale : _originalScale;
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        _scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
        
        if (_buttonImage != null)
        {
            Color targetColor = hover ? hoverColor : normalColor;
            if (_colorCoroutine != null)
            {
                StopCoroutine(_colorCoroutine);
            }
            _colorCoroutine = StartCoroutine(AnimateColor(targetColor));
        }
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Ease out back
            t = 1f - t;
            t = 1f - (t * t * t);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }

    private IEnumerator AnimateColor(Color targetColor)
    {
        Color startColor = _buttonImage.color;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            _buttonImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        _buttonImage.color = targetColor;
    }

    private void PlayHoverSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(hoverSoundName);
        }
    }

    private void PlayClickSound()
    {
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(clickSoundName))
        {
            SoundManager.Instance.Play(clickSoundName);
        }
    }

    private void OnDestroy()
    {
        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
        }
        if (_colorCoroutine != null)
        {
            StopCoroutine(_colorCoroutine);
        }
    }
}
