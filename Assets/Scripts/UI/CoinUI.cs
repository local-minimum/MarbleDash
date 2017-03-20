using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour {

    [SerializeField]
    Text coinField;
    
    private void OnEnable()
    {

        PlayerRunData.stats.OnCoinChange += Stats_OnCoinChange;
    }

    private void OnDisable()
    {
        PlayerRunData.stats.OnCoinChange -= Stats_OnCoinChange;
    }

    private void OnDestroy()
    {
        PlayerRunData.stats.OnCoinChange -= Stats_OnCoinChange;
    }

    private void Stats_OnCoinChange(int from, int to)
    {
        if (from == to)
        {
            coinField.text = to.ToString();
        } else if (to == 0)
        {
            coinField.text = to.ToString();
        } else
        {
            StartCoroutine(ChangeCoin(from, to));
        }
    }

    int toValue;
    bool counting = false;

    [SerializeField]
    float countWait = 0.1f;

    IEnumerator<WaitForSeconds> ChangeCoin(int fromValue, int toValue)
    {
        this.toValue = toValue;
        if (!counting)
        {
            counting = true;

            while (fromValue != this.toValue)
            {
                if (fromValue > this.toValue)
                {
                    fromValue--;
                } else
                {
                    fromValue++;
                }
                coinField.text = fromValue.ToString();
                yield return new WaitForSeconds(countWait);
            }

            counting = false;
        }
    }
}
