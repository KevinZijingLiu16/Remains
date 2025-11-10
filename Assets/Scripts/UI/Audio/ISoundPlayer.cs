using UnityEngine;

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
    void PlayScaled(string soundName, float volumeScale = 1f);
    void PlayOneShot3D(string soundName, Transform attachTo, float volume = 1f,
                         float minDistance = 1f, float maxDistance = 15f);

    void PlayNamedLoop(string identifier, string soundName, float volume = 1f,
                       Transform follow = null, float spatialBlend = 0f,
                       float minDistance = 1f, float maxDistance = 15f);
}