using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class FoamPlatform : MonoBehaviour, IFoamPlatform
{
    [Header("Platform Settings")]
    public float lifetime = 30f; 
    public float stepHeight = 0.5f; 
    public bool canStack = true; 
    public LayerMask playerLayer = 1; 

    [Header("Visual")]
    public Material foamMaterial;
    public Color foamColor = Color.white;

    [Header("Physics")]
    public float platformFriction = 0.8f;
    public bool solidifyOnLanding = true; 

    private bool _isSolidified = false;
    private float _creationTime;
    private Collider _collider;
    private Rigidbody _rigidbody;
    private Renderer _renderer;

    public bool IsSteppable => _isSolidified;
    public float PlatformHeight => stepHeight;

    void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
        _creationTime = Time.time;

        SetupFoamPhysics();
        SetupVisuals();
    }

    void Start()
    {
       
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
       
        if (!_isSolidified && solidifyOnLanding && _rigidbody.linearVelocity.magnitude < 0.1f)
        {
            SolidifyPlatform();
        }

       
        UpdateVisualFeedback();
    }

    private void SetupFoamPhysics()
    {
       
        _rigidbody.mass = 0.1f;
        _rigidbody.linearDamping = 2f;
        _rigidbody.angularDamping = 5f;

       
        _collider.isTrigger = false;

        
        PhysicsMaterial foamPhysics = new PhysicsMaterial("FoamPhysics");
        foamPhysics.dynamicFriction = platformFriction;
        foamPhysics.bounciness = 0.3f;
        _collider.material = foamPhysics;
    }

    private void SetupVisuals()
    {
        if (_renderer == null) return;

    
        if (foamMaterial != null)
        {
            _renderer.material = foamMaterial;
        }

       
        if (_renderer.material.HasProperty("_Color"))
        {
            _renderer.material.color = foamColor;
        }
    }

    private void SolidifyPlatform()
    {
        if (_isSolidified) return;

        _isSolidified = true;

        
        _rigidbody.mass = 1f;
        _rigidbody.linearDamping = 5f;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

       
        Debug.Log($"[FoamPlatform] Platform solidified at {transform.position}");
    }

    private void UpdateVisualFeedback()
    {
        if (_renderer == null || !_renderer.material.HasProperty("_Color")) return;

       
        float remainingTime = lifetime - (Time.time - _creationTime);
        float alpha = Mathf.Clamp01(remainingTime / lifetime);

       
        if (remainingTime < 5f)
        {
            alpha *= (Mathf.Sin(Time.time * 10f) * 0.3f + 0.7f);
        }

        Color currentColor = foamColor;
        currentColor.a = alpha;
        _renderer.material.color = currentColor;
    }

    void OnCollisionEnter(Collision collision)
    {
       
        if (IsInLayerMask(collision.gameObject.layer, playerLayer))
        {
            OnPlayerStepped();
        }

        
        if (canStack)
        {
            var otherPlatform = collision.gameObject.GetComponent<FoamPlatform>();
            if (otherPlatform != null)
            {
                HandlePlatformStacking(otherPlatform);
            }
        }
    }

    public void OnPlayerStepped()
    {
        if (!_isSolidified)
        {
            SolidifyPlatform();
        }

        Debug.Log("[FoamPlatform] Player stepped on foam platform");

       
    }

    private void HandlePlatformStacking(FoamPlatform otherPlatform)
    {
       
        if (otherPlatform._creationTime < _creationTime)
        {
            SolidifyPlatform();
        }
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

   
    public void ForceSolidify()
    {
        SolidifyPlatform();
    }

   
    public void ExtendLifetime(float additionalTime)
    {
        lifetime += additionalTime;
    }
}