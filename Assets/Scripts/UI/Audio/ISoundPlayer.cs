public interface ISoundPlayer
{
    void Play(string soundName);
    void PlayLoop(string soundName, float volume = 1f);
    void StopLoop();
    void SetLoopVolume(float volume);
    void PlayNamedLoop(string identifier, string soundName, float volume = 1f);
    void StopNamedLoop(string identifier);
    void SetNamedLoopVolume(string identifier, float volume);
    void SetVolume(float volume);
    void SetBGMVolume(float volume);
    float GetVolume();
    float GetBGMVolume();
}