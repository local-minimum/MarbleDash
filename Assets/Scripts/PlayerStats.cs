using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CoinChange(int from, int to);

[System.Serializable]
public class PlayerStats {

    public event CoinChange OnCoinChange;

    [SerializeField]
    int coin;
    
    public int Coin
    {
        get
        {
            return coin;
        }

        set
        {
            if (OnCoinChange != null)
            {
                OnCoinChange(coin, value);
            }
            coin = value;
        }
    }

    public void Reset()
    {
        Coin = 0;
    }
}
