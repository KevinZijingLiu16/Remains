public class LightTrap : BaseTrap
{
    void Awake()
    {
        damageAmount = 1;
        hitEffect.gameObject.SetActive(false);
    }
}