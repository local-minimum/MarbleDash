using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelOrStoreUI : MonoBehaviour {

    [SerializeField]
    Text target;

    bool outOfSync = true;

    Level lvl;

    private void OnEnable()
    {
        lvl = Level.instance;
        if (lvl)
        {
            lvl.OnNewLevel += SetTextFromLevel;
        }
    }

    private void OnDisable()
    {
        if (lvl)
        {
            lvl.OnNewLevel -= SetTextFromLevel;
        }
    }

    void Update () {
	    if (outOfSync)
        {
            if (PlayerRunData.stats.InStore)
            {
                SetTextFromStore();
            } else
            {
                SetTextFromLevel();
            }
        }
	}


    void SetTextFromLevel()
    {
        Debug.Log("Updating lvl ui text");
        outOfSync = false;
        target.text = "Level " + PlayerRunData.stats.currentLevel;
    }

    void SetTextFromStore()
    {
        outOfSync = true;
        target.text = "No-name store";
    }
}
