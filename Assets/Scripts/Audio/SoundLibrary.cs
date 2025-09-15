using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<Sound> sounds = new();

    private Dictionary<string, AudioClip> _soundDict;

    private void OnEnable()
    {
        _soundDict = new();
        foreach (var sound in sounds)
        {
            if (!_soundDict.ContainsKey(sound.name))
                _soundDict.Add(sound.name, sound.clip);
        }
    }

    public AudioClip GetClip(string name)
    {
        return _soundDict.TryGetValue(name, out var clip) ? clip : null;
    }
}