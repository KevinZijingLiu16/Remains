public interface IFoamPlatform
{
    bool IsSteppable { get; }
    float PlatformHeight { get; }
    void OnPlayerStepped();
}