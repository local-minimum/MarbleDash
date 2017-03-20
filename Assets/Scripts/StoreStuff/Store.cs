using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Store : MonoBehaviour {

    static Store _instance;

    static Store instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Store>();
            }
            return _instance;
        }
    }

    static int displayingIndex = -2;

    static float lastTrigger;

    public static void Display(int index)
    {
        lastTrigger = Time.timeSinceLevelLoad;

        if (index != displayingIndex)
        {
            displayingIndex = index;
            instance.ShowMessage();
        }
    }

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
            FindStoreItems();
        } else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    [SerializeField]
    Text coins;
    
    [SerializeField]
    Image img;

    [SerializeField]
    Sprite itemFront;

    [SerializeField]
    Sprite emptySlot;

    [SerializeField]
    Sprite[] generalSprites;

    [SerializeField]
    float revertTime;

    [SerializeField]
    Text displayTitle;

    [SerializeField]
    Text displayDesc;

    [SerializeField]
    Text displayPrize;

    [SerializeField]
    Image displayIcon;

    [SerializeField]
    string pathToStoreItems;

    private void OnEnable()
    {
        EquipStore();
    }

    private void Start()
    {
        coins.text = PlayerRunData.stats.Coin.ToString();
        displayingIndex = -2;
        ShowMessage();
    }

    static StoreItem[] items = null;
    static float itemAccumulatedProb = 0;
    int freeSlots = 4;

    void FindStoreItems()
    {
        if (items != null)
        {
            return;
        }
       
        items = Resources.LoadAll<StoreItem>(pathToStoreItems);
        Debug.Log(items.Length + " store items found at: " + pathToStoreItems);
        itemAccumulatedProb = 0;
        for (int i = 0; i < items.Length; i++)
        {
            itemAccumulatedProb += items[i].availabilityProbability;
        }
    }

    Dictionary<int, StoreItem> storeItemSlots = new Dictionary<int, StoreItem>();

    void EquipStore()
    {
        storeItemSlots.Clear();

        float remainingAccProb = itemAccumulatedProb;

        for (int storeSlotIndex=0; storeSlotIndex<freeSlots; storeSlotIndex++)
        {
            if (remainingAccProb <= 0)
            {
                break;
            }

            float p = PlayerRunData.stats.storeRnd.Range(0, remainingAccProb);

            for (int storeItemIndex = 0; storeItemIndex < items.Length; storeItemIndex++)
            {
                if (storeItemSlots.ContainsValue(items[storeItemIndex]))
                {
                    continue;
                }

                float itemP = items[storeItemIndex].availabilityProbability;
                if (p <= itemP)
                {
                    storeItemSlots[storeSlotIndex] = items[storeItemIndex];
                    remainingAccProb -= itemP;
                    break;
                } else
                {
                    p -= itemP;
                }
            }
        }
    } 

    void ShowMessage()
    {
        if (displayingIndex < 0)
        {

            img.sprite = generalSprites[Mathf.Abs(displayingIndex) - 1];
            img.preserveAspect = true;
            SetVisibilities(false);

        } else
        {
            if (storeItemSlots.ContainsKey(displayingIndex))
            {
                StoreItem sItem = storeItemSlots[displayingIndex];
                img.sprite = itemFront;
                displayIcon.sprite = sItem.storeIcon;
                displayTitle.text = sItem.itemTitle;
                displayPrize.text = sItem.baseCost.ToString();
                displayDesc.text = sItem.itemDescription;                
                SetVisibilities(true);
            } else
            {
                img.sprite = emptySlot;
                SetVisibilities(false);
            }

        }
    }

    void SetVisibilities(bool isItem)
    {
        displayTitle.enabled = isItem;
        displayIcon.enabled = isItem;
        displayDesc.enabled = isItem;
        displayPrize.enabled = isItem;
    }

    private void Update()
    {
        if (displayingIndex != -2 && Time.timeSinceLevelLoad - lastTrigger > revertTime)
        {
            displayingIndex = -2;
            ShowMessage();
        }
    }
}
