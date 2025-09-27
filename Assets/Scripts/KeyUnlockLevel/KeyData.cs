[System.Serializable]
public class KeyData
{
    public string keyID;
    public string unlocksLevel;
    public long collectionTime;
}

public static class KeyEvents
{
    public static System.Action<string, string> OnKeyCollected;
}