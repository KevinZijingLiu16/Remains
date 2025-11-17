using UnityEngine;
using UnityEngine.UI;

public class PowerBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerPower playerPower;   // 拖 Player 对象（含 PlayerPower 脚本）
    [SerializeField] private Image fillImage;           // 拖 UI Image（Fill）

    [Header("Color Settings (Editable in Inspector)")]
    [SerializeField] private Color highColor = Color.green;  // 100~70%
    [SerializeField] private Color midColor = Color.yellow;  // 70~20%
    [SerializeField] private Color lowColor = Color.red;     // 20~0%

    [Header("Thresholds (Editable)")]
    [Range(0, 100)][SerializeField] private int midThreshold = 70;
    [Range(0, 100)][SerializeField] private int lowThreshold = 20;

    private void Start()
    {
        if (playerPower == null)
            playerPower = FindFirstObjectByType<PlayerPower>();

        if (playerPower != null)
            playerPower.OnPowerChanged += UpdatePowerBar;

        // 初始化显示
        UpdatePowerBar(playerPower.Current, playerPower.Max);
    }

    private void OnDestroy()
    {
        if (playerPower != null)
            playerPower.OnPowerChanged -= UpdatePowerBar;
    }

    private void UpdatePowerBar(int current, int max)
    {
        if (fillImage == null || max <= 0) return;

        float percent = (float)current / max;
        fillImage.fillAmount = percent;

        // 改变颜色
        if (percent > (float)midThreshold / 100f)
        {
            fillImage.color = highColor;
        }
        else if (percent > (float)lowThreshold / 100f)
        {
            fillImage.color = midColor;
        }
        else
        {
            fillImage.color = lowColor;
        }
    }
}
