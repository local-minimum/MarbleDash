using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour {

    [SerializeField]
    Text coinField;

    PlayerController player;
    private void OnEnable()
    {
        player = PlayerController.instance;
        player.Stats.OnCoinChange += Stats_OnCoinChange;
    }

    private void OnDisable()
    {
        player.Stats.OnCoinChange -= Stats_OnCoinChange;
    }

    private void Stats_OnCoinChange(int from, int to)
    {
        if (from == to)
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
