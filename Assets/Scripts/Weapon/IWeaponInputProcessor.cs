public interface IWeaponInputProcessor
{
    void ProcessInput(IWeaponAttackBehavior primaryAttack, IWeaponAttackBehavior secondaryAttack);
    event System.Action OnPrimaryAttackStart;
    event System.Action OnPrimaryAttackStop;
    event System.Action OnSecondaryAttackStart;
    event System.Action OnSecondaryAttackStop;
}