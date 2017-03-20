using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerRunData {

    static bool isLoaded = false;

    static void LoadStats()
    {
        _stats = Resources.Load<PlayerStats>("PlayerData/PlayerStats");
        if (!_stats.validSave)
        {
            _stats.Reset();
        }
        isLoaded = true;
    }

    static PlayerStats _stats;

    public static PlayerStats stats
    {
        get
        {
            if  (!isLoaded)
            {
                LoadStats();  
            }
            return _stats;
        }
    } 
}
