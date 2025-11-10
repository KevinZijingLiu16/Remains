using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
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
        
        transform.DOScale(hover ? _originalScale * hoverScale : _originalScale, animationDuration)
            .SetEase(Ease.OutBack);
        
        if (_buttonImage != null)
        {
            _buttonImage.DOColor(hover ? hoverColor : normalColor, animationDuration);
        }
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
        transform.DOKill();
        if (_buttonImage != null)
        {
            _buttonImage.DOKill();
        }
    }
}
