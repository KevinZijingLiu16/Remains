public class HeavyTrap : BaseTrap
{
    void Awake()
    {
        damageAmount = 3;
    }

    protected override void OnDamageDealt()
    {
        base.OnDamageDealt();
  
    }
}