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
    Image img;

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

    private void Start()
    {
        displayingIndex = -2;
        ShowMessage();
    }

    void ShowMessage()
    {
        if (displayingIndex < 0)
        {
            img.sprite = generalSprites[Mathf.Abs(displayingIndex) - 1];
            img.preserveAspect = true;
        }   SetVisibilities(false);
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
