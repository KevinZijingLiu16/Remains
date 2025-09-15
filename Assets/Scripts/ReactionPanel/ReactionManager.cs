using System.Collections.Generic;
using UnityEngine;

public class ReactionManager : MonoBehaviour, IReactionService
{
    public static IReactionService Instance { get; private set; }

    [SerializeField] private ReactionLibrary library;
    [SerializeField] private GameObject defaultPresenterPrefab;

    private readonly Dictionary<GameObject, IReactionPresenter> _presenterCache = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool ShowReaction(GameObject target, string key, ReactionOptions? options = null)
    {
        if (target == null || string.IsNullOrWhiteSpace(key)) return false;
        if (library == null) return false;
        if (!library.TryGet(key, out var sprite) || sprite == null) return false;

        var presenter = ResolvePresenter(target);
        if (presenter == null) return false;

        var opt = options ?? ReactionOptions.Default;
        presenter.ShowReaction(sprite, opt);
        return true;
    }

    public bool ShowReaction(IReactionPresenter presenter, string key, ReactionOptions? options = null)
    {
        if (presenter == null || string.IsNullOrWhiteSpace(key)) return false;
        if (library == null) return false;
        if (!library.TryGet(key, out var sprite) || sprite == null) return false;

        var opt = options ?? ReactionOptions.Default;
        presenter.ShowReaction(sprite, opt);
        return true;
    }

    public void HideReaction(GameObject target)
    {
        var p = ResolvePresenter(target, false);
        p?.HideReaction();
    }

    public void HideReaction(IReactionPresenter presenter)
    {
        presenter?.HideReaction();
    }

    private IReactionPresenter ResolvePresenter(GameObject target, bool createIfMissing = true)
    {
        if (target == null) return null;

        if (_presenterCache.TryGetValue(target, out var cached) && cached != null)
            return cached;

        var presenter = target.GetComponentInChildren<IReactionPresenter>(true);

        if (presenter == null && createIfMissing && defaultPresenterPrefab != null)
        {
            var go = Instantiate(defaultPresenterPrefab, target.transform);
            presenter = go.GetComponentInChildren<IReactionPresenter>(true);
        }

        if (presenter != null)
            _presenterCache[target] = presenter;

        return presenter;
    }

    public void UnregisterPresenterOwner(GameObject owner)
    {
        if (owner != null) _presenterCache.Remove(owner);
    }
}
