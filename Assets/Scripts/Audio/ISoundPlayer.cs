public interface ISoundPlayer
{
    void Play(string soundName);
    void SetVolume(float volume);
    float GetVolume();

  
    void PlayLoop(string soundName, float volume = 1f);
    void StopLoop();
    void SetLoopVolume(float volume);
}