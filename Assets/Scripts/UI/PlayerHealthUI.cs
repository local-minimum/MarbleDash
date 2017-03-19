using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour {

    [SerializeField]
    Destructable playerDestructable;

    [SerializeField]
    string showTrigger;

    [SerializeField]
    float adjustHealthTime = 0.5f;

    [SerializeField]
    Image barImage;

    Animator anim;

    [SerializeField]
    float initialDelay = 2f;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }


    float sourceHealth;

    IEnumerator<WaitForSeconds> AdjustHealth(float initialDelay)
    {

        yield return new WaitForSeconds(initialDelay);

        anim.SetTrigger(showTrigger);

        float targetHealth = playerDestructable.PartialHealth;
        float startT = Time.timeSinceLevelLoad;
        float t = 0;
        while (t < 1)
        {
            t = (Time.timeSinceLevelLoad - startT) / adjustHealthTime;
            barImage.fillAmount = Mathf.Lerp(sourceHealth, targetHealth, t);    
            yield return new WaitForSeconds(0.016f);
        }
        barImage.fillAmount = targetHealth;
        sourceHealth = targetHealth;
    }

    private void OnEnable()
    {
        playerDestructable.OnHealthChange += PlayerDestructable_OnHealthChange;
    }

    private void OnDisable()
    {
        playerDestructable.OnHealthChange -= PlayerDestructable_OnHealthChange;
    }

    private void PlayerDestructable_OnHealthChange()
    {
        if (Level.LevelRunning)
        {
            StartCoroutine(AdjustHealth(0));
        }
    }

}
