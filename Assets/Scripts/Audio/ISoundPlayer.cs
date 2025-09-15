public interface ISoundPlayer
{
    void Play(string soundName);
    void SetVolume(float volume);
    float GetVolume();
}