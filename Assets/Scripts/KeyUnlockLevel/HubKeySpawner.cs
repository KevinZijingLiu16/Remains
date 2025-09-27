using UnityEngine;
using System.Collections.Generic;

public class HubKeySpawner : MonoBehaviour
{
    [Header("Hub Key Settings")]
    [SerializeField] private List<HubKeyData> hubKeys = new List<HubKeyData>();

    void Start()
    {
        SpawnKeys();
    }

    private void SpawnKeys()
    {
        foreach (var keyData in hubKeys)
        {
           
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.HasKey(keyData.keyID))
            {
               
                continue;
            }

         
            if (keyData.keyPrefab != null)
            {
                GameObject keyObj = Instantiate(keyData.keyPrefab, keyData.spawnPosition, keyData.spawnRotation);
                KeyCollectible keyCollectible = keyObj.GetComponent<KeyCollectible>();
                if (keyCollectible == null)
                {
                    keyCollectible = keyObj.AddComponent<KeyCollectible>();
                }

               
                keyCollectible.SetKeyData(keyData.keyID, keyData.unlocksLevel);
            }
        }
    }

    [System.Serializable]
    public class HubKeyData
    {
        public string keyID;
        public string unlocksLevel;
        public GameObject keyPrefab;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation = Quaternion.identity;
    }
}