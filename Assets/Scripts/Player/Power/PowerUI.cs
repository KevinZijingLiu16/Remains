using UnityEngine;
using UnityEngine.UI;

public class PowerUI : MonoBehaviour
{
    [SerializeField] private Slider powerSlider;

    void Start()
    {
        var playerPower = FindFirstObjectByType<PlayerPower>();
        if (playerPower == null)
        {
            Debug.LogWarning("[PowerUI] No PlayerPower found in scene.");
            return;
        }
       
        playerPower.OnPowerChanged += UpdateUI;

  
        powerSlider.maxValue = playerPower.Max;
        powerSlider.value = playerPower.Current;
    }

    void UpdateUI(int current, int max)
    {
        powerSlider.maxValue = max;
        powerSlider.value = current;
    }
}
