using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayModeHoles { InstaKill, Damage};

public static class RndHelper
{
    public static float Range(this System.Random rnd, float low, float high)
    {
        return low + (float)(rnd.NextDouble() * (high - low));
    }

    public static int Range(this System.Random rnd, int low, int high)
    {
        return low + rnd.Next(high - low);
    }
}

public delegate void CoinChange(int from, int to);

[CreateAssetMenu(fileName = "PlayerStats", menuName = "PlayerRunStats", order = 0)]
public class PlayerStats : ScriptableObject {
    
    public bool validSave = false;

    #region Game

    public PlayModeHoles holeMode = PlayModeHoles.Damage;

    public int holeDamage = 10;

    [SerializeField]
    int gameRandomSeed;

    [SerializeField]
    System.Random _gameRandomSouce = null;
    
    System.Random gameRandomSource
    {
        get
        {
            if (_gameRandomSouce == null)
            {
                gameRandomSeed = Random.Range(0, 100000);
                _gameRandomSouce = new System.Random(gameRandomSeed);
            }
            return _gameRandomSouce;
        }
    }
    #endregion

    #region Level
    System.Random _lvlRnd;

    public System.Random lvlRnd
    {
        get
        {
            return _lvlRnd;
        }
    }

    [SerializeField]
    int _currentLevel;

    public int currentLevel
    {
        get
        {
            return _currentLevel;
        }
    }

    [SerializeField]
    int _lvlRndSeed;

    public void NextLevel()
    {
        _lvlRndSeed = gameRandomSource.Next(100000);
        _lvlRnd = new System.Random(_lvlRndSeed);
        _storeRnd = new System.Random(_lvlRndSeed);

        _currentLevel++;
    }
    
    public int damageTaken;
    #endregion

    #region Stores
    [SerializeField]
    System.Random _storeRnd;

    public System.Random storeRnd
    {
        get
        {
            return _storeRnd;
        }
    }


    [SerializeField]
    int storesEntered;

    [SerializeField]
    bool inStore;

    public bool InStore
    {
        get
        {
            return inStore;
        }
    }

    public void EnterNewStore()
    {
        storesEntered++;
        inStore = true;
    }

    public void Bought(StoreItem item)
    {
        Coin -= item.baseCost;
        Debug.Log(string.Format("Bought {0} for {1}", item.name, item.baseCost));
        if (item.slottable)
        {
            SetSlot(item);
        } else
        {
            foreach (var effect in item.effects)
            {
                switch (effect.effectProperty)
                {
                    case "Health":
                        Health += Mathf.FloorToInt(effect.effectMagnitude);
                        break;
                    default:
                        Debug.Log("unkown effect '" + effect.effectProperty + "' from " + item.name);
                        break;
                }
            }
        }
    }

    public void ExitStore()
    {
        inStore = false;
    }

    #endregion

    #region Coins

    public event CoinChange OnCoinChange;

    [SerializeField]
    int coin;

    [SerializeField]
    int totalCoin;

    [SerializeField]
    int coinSpent;

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
            if (value > coin)
            {
                totalCoin += value - coin;
            } else if (coin > value)
            {
                coinSpent += coin - value;
            }
            coin = value;
        }
    }

    #endregion

    #region Health
    [SerializeField]
    int currentHealth;

    [SerializeField]
    int maxHealth = 20;

    public int Health
    {
        get
        {
            return currentHealth;
        }

        set
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
        }
    }

    public float PartialHealth
    {
        get
        {
            return currentHealth / (float) maxHealth;
        }
    }

    #endregion

    #region ItemSlot
    [SerializeField, HideInInspector]
    StoreItem _slot;


    void SetSlot(StoreItem item)
    {
        _slot = item;
    } 
    #endregion

    public void Reset()
    {
        _gameRandomSouce = null;
        holeDamage = 10;

        coin = 0;
        totalCoin = 0;
        coinSpent = 0;
        if (OnCoinChange != null)
        {
            OnCoinChange(0, 0);
        }

        storesEntered = 0;
        inStore = false;

        damageTaken = 0;

        maxHealth = 20;
        currentHealth = maxHealth;


        _currentLevel = 0;
        NextLevel();
    }
}
