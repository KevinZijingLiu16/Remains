public interface IFoamAffectable
{
    void ApplyFoamSlow(float slowAmount, float duration);
    void RemoveFoamSlow();
}