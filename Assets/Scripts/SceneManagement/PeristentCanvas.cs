// PersistentCanvas.cs
using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}