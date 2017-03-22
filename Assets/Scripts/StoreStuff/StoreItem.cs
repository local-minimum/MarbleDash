using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreItem", menuName = "StoreItem", order = 0)]
public class StoreItem : ScriptableObject {

    public string itemTitle;
    public string itemDescription;

    public Sprite storeIcon;
    public Sprite slottedIcon;
    public string slottedAnimation;

    public int baseCost;

    public bool slottable;
    public float availabilityProbability;

    public List<StoreItemEffect> effects = new List<StoreItemEffect>();

}

[System.Serializable]
public struct StoreItemEffect
{
    public string effectName;
    public string effectProperty;
    public float effectMagnitude;
}
