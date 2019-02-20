using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Converts item keys into their corresponding prefab.
/// </summary>

public class KeyToItem : MonoBehaviour
{
    public GameObject heartPrefab;
    public GameObject coinPrefab;
    public GameObject keyPrefab;
    public GameObject redGemPrefab;
    public GameObject greenGemPrefab;
    public GameObject blueGemPrefab;
    public GameObject yellowGemPrefab;
    public GameObject gunPickupPrefab;
    

    public GameObject ConvertToPrefab(string key)
    {
        switch (key)
        {
            case "heart":
                return heartPrefab;
            case "coin":
                return coinPrefab;
            case "key":
                return keyPrefab;
            case "gem_red":
                return redGemPrefab;
            case "gem_green":
                return greenGemPrefab;
            case "gem_blue":
                return blueGemPrefab;
            case "gem_yellow":
                return yellowGemPrefab;
            case "gunPickup":
                return gunPickupPrefab;
            
            default:
                Debug.Log("Missing prefab or incorrect key.");
                return null;
        }
    }
}
