// IReactionService.cs
using UnityEngine;

public interface IReactionService
{
    bool ShowReaction(GameObject target, string key, ReactionOptions? options = null);
    bool ShowReaction(IReactionPresenter presenter, string key, ReactionOptions? options = null);
    void HideReaction(GameObject target);
    void HideReaction(IReactionPresenter presenter);
}

public interface IReactionLibrary
{
    bool TryGet(string key, out Sprite sprite);
}

public interface IReactionPresenter
{
    Transform Anchor { get; }                
    bool IsBusy { get; }                     
    int CurrentPriority { get; }
    void ShowReaction(Sprite sprite, in ReactionOptions options);
    void HideReaction();
}


[System.Serializable]
public struct ReactionOptions
{
    public float duration;                   
    public Vector2 screenOffset;             
    public int priority;                     
    public bool overrideIfLowerPriority;     
    public bool queueIfBusy;                 
    public static ReactionOptions Default => new ReactionOptions
    {
        duration = 1.5f,
        screenOffset = Vector2.zero,
        priority = 0,
        overrideIfLowerPriority = false,
        queueIfBusy = false
    };
}
