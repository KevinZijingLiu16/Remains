using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HeadFaceEmissionByPower : MonoBehaviour
{
    [Header("Power")]
    [SerializeField] private PlayerPower playerPower;   
    [SerializeField] private int lowPowerThreshold = 20; 

    [Header("Target Material")]

    [SerializeField] private int materialIndex = 1;

    [Header("Low Power Emission")]
    [SerializeField] private Color lowColor = Color.red;  
    [SerializeField] private float lowIntensity = 2.0f;  

    private Renderer _rend;
    private Material _mat;              
    private Color _originalEmissionColor;
    private Texture _originalEmissionMap;
    private bool _hadEmissionKeyword;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissionMapID = Shader.PropertyToID("_EmissionMap");

    void Awake()
    {
        _rend = GetComponent<Renderer>();
        if (!playerPower) playerPower = GetComponentInParent<PlayerPower>();

  
        var mats = _rend.materials;
        if (materialIndex < 0 || materialIndex >= mats.Length)
        {
        
            enabled = false; return;
        }
        _mat = mats[materialIndex];

     
        _originalEmissionColor = _mat.HasProperty(EmissionColorID) ? _mat.GetColor(EmissionColorID) : Color.black;
        _originalEmissionMap = _mat.HasProperty(EmissionMapID) ? _mat.GetTexture(EmissionMapID) : null;
        _hadEmissionKeyword = _mat.IsKeywordEnabled("_EMISSION");
    }

    void OnEnable()
    {
        if (playerPower != null)
            playerPower.OnPowerChanged += OnPowerChanged;

    
        ApplyByPower(playerPower ? playerPower.Current : int.MaxValue);
    }

    void OnDisable()
    {
        if (playerPower != null)
            playerPower.OnPowerChanged -= OnPowerChanged;
    }

    private void OnPowerChanged(int current, int max)
    {
        ApplyByPower(current);
    }

    private void ApplyByPower(int current)
    {
        bool low = current <= lowPowerThreshold;

        if (low)
        {
          
            _mat.EnableKeyword("_EMISSION");
            _mat.SetColor(EmissionColorID, lowColor * Mathf.LinearToGammaSpace(lowIntensity));
         
        }
        else
        {
           
            _mat.SetColor(EmissionColorID, _originalEmissionColor);
            _mat.SetTexture(EmissionMapID, _originalEmissionMap);
            if (!_hadEmissionKeyword)
                _mat.DisableKeyword("_EMISSION");
        }
     
    }
}
