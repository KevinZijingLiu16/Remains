public interface IHealthRestore
{
    int RestoreAmount { get; }
    void RestoreHealth(IHealth target);
}