using UnityEngine;
using System.Collections;

public class EnemyFoamSlow : MonoBehaviour, IFoamAffectable
{
    [Header("Foam Slow Settings")]
    public float maxSlowAmount = 0.5f; 
    public float defaultSlowDuration = 5f; 

    [Header("Visual Feedback")]
    public GameObject slowEffectPrefab; 
    public Color slowColor = Color.cyan; 

    private float _originalSpeed;
    private float _currentSlowAmount = 0f;
    private bool _isSlowed = false;
    private Coroutine _slowCoroutine;
    private GameObject _slowEffect;


    private UnityEngine.AI.NavMeshAgent _navAgent;
    private Rigidbody _rigidbody;
    private Renderer _renderer;
    private Material _originalMaterial;

    void Awake()
    {
      
        _navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();

        
        if (_navAgent != null)
        {
            _originalSpeed = _navAgent.speed;
        }
        else if (_rigidbody != null)
        {
            
            _originalSpeed = 5f; 
        }

      
        if (_renderer != null)
        {
            _originalMaterial = _renderer.material;
        }
    }

    public void ApplyFoamSlow(float slowAmount, float duration)
    {
    
        if (_isSlowed && slowAmount <= _currentSlowAmount)
        {
            return; 
        }

       
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
        }

        _currentSlowAmount = Mathf.Clamp01(slowAmount);
        _isSlowed = true;

     
        ApplySpeedReduction();

      
        ApplyVisualEffects();

      
        _slowCoroutine = StartCoroutine(SlowTimer(duration));

        Debug.Log($"[EnemyFoamSlow] Applied {slowAmount * 100}% slow for {duration}s to {gameObject.name}");
    }

    public void RemoveFoamSlow()
    {
        if (!_isSlowed) return;

        _isSlowed = false;
        _currentSlowAmount = 0f;

    
        if (_slowCoroutine != null)
        {
            StopCoroutine(_slowCoroutine);
            _slowCoroutine = null;
        }

       
        RestoreOriginalSpeed();

     
        RemoveVisualEffects();

        Debug.Log($"[EnemyFoamSlow] Removed slow effect from {gameObject.name}");
    }

    private void ApplySpeedReduction()
    {
        float speedMultiplier = 1f - _currentSlowAmount;

        if (_navAgent != null)
        {
            _navAgent.speed = _originalSpeed * speedMultiplier;
        }

    }

    private void RestoreOriginalSpeed()
    {
        if (_navAgent != null)
        {
            _navAgent.speed = _originalSpeed;
        }

    }

    private void ApplyVisualEffects()
    {
      
        if (slowEffectPrefab != null && _slowEffect == null)
        {
            _slowEffect = Instantiate(slowEffectPrefab, transform);
        }

    
        if (_renderer != null && _renderer.material.HasProperty("_Color"))
        {
            Color originalColor = _originalMaterial.color;
            Color blendedColor = Color.Lerp(originalColor, slowColor, _currentSlowAmount);
            _renderer.material.color = blendedColor;
        }
    }

    private void RemoveVisualEffects()
    {
        
        if (_slowEffect != null)
        {
            Destroy(_slowEffect);
            _slowEffect = null;
        }

        
        if (_renderer != null && _originalMaterial != null)
        {
            _renderer.material.color = _originalMaterial.color;
        }
    }

    private IEnumerator SlowTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveFoamSlow();
    }

    void OnDestroy()
    {
     
        if (_slowEffect != null)
        {
            Destroy(_slowEffect);
        }
    }
}