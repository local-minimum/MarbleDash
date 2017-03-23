using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoreSwapper : MonoBehaviour {

    [SerializeField]
    Transform holesParent;

    [SerializeField]
    float remainTime = 5;

    bool storeSwappingAcitve = false;

    static StoreSwapper _instance;

    public static StoreSwapper instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StoreSwapper>();
            }
            return _instance;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Update () {
        if (storeSwappingAcitve && Time.timeSinceLevelLoad - lastChange > remainTime)
        {
            SwapStore();
        } else if (storeSwappingAcitve && 
            !swapWarned && 
            remainTime - (Time.timeSinceLevelLoad - lastChange) < swapWarnAhead)
        {
            WarnForSwap();
        } else if (!storeSwappingAcitve)
        {
            DisableStore();
        }
	}

    float lastChange;

    int activeStoreIndex = -1;
    StoreTrigger activeStore;

    [SerializeField]
    int swapWarningEmision = 100;

    [SerializeField]
    float swapWarnAhead = 0.75f;

    [SerializeField]
    Color swapWarnColor = Color.red;

    bool swapWarned;

    void WarnForSwap()
    {
        activeStore.EmitExtra(swapWarningEmision, swapWarnColor);
        swapWarned = true;
    }

    void SwapStore()
    {
        DisableStore();
        List<int> available = new List<int>();
        for (int i=0; i<holesParent.childCount; i++)
        {
            if (holesParent.GetChild(i).gameObject.activeSelf && i != activeStoreIndex)
            {
                available.Add(i);
            }
        }

        activeStoreIndex = available[Random.Range(0, available.Count)];
        Transform holeTile = holesParent.GetChild(activeStoreIndex);
        activeStore = holeTile.GetComponentInChildren<StoreTrigger>(true);
        activeStore.StartStoreTrigger();
        lastChange = Time.timeSinceLevelLoad;
        swapWarned = false;
    }

    void DisableStore()
    {
        if (activeStore)
        {
            activeStore.StopStoreTrigger();
        }
    }

    public void EnterStore()
    {
        storeSwappingAcitve = false;
        activeStoreIndex = -1;
        Level.instance.StopTheMotion();
        PlayerRunData.stats.EnterNewStore();
        SceneManager.LoadScene("store");
        Debug.Log("Enter Store");
    }

    public void ShowAllStores()
    {
        for (int i = 0; i < holesParent.childCount; i++)
        {
            if (holesParent.GetChild(i).gameObject.activeSelf)
            {
                holesParent.GetChild(i).GetComponentInChildren<StoreTrigger>(true).StartStoreTrigger();

            }
        }
    }

    public void ShowStoreSwapping()
    {
        activeStoreIndex = -1;
        lastChange = Time.timeSinceLevelLoad - remainTime;
        storeSwappingAcitve = true;
    }

    public void HideAllStores()
    {
        storeSwappingAcitve = false;
        DisableStore();
    }
}
