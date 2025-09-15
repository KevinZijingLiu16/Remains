// ReactionLibrary.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Reactions/Reaction Library")]
public class ReactionLibrary : ScriptableObject, IReactionLibrary
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public Sprite sprite;
    }

    [SerializeField] private List<Entry> entries = new();
    private Dictionary<string, Sprite> _dict;

    void OnEnable()
    {
        _dict = new Dictionary<string, Sprite>();
        foreach (var e in entries)
        {
            if (!string.IsNullOrWhiteSpace(e.key) && e.sprite != null)
                _dict[e.key] = e.sprite;
        }
    }

    public bool TryGet(string key, out Sprite sprite)
    {
        if (_dict == null) OnEnable();
        return _dict.TryGetValue(key, out sprite);
    }
}
