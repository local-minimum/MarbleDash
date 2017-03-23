using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer && gameObject.activeSelf)
        {
            if (PlayerRunData.stats.InStore)
            {
                Store.instance.BuyOrExit();
            }
            else
            {
                StoreSwapper.instance.EnterStore();
            }
        }
    }

    int playerLayer;
    ParticleSystem ps;


    void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");
        if (ps == null)
        {
            ps = GetComponent<ParticleSystem>();
        }
        if (PlayerRunData.stats.InStore)
        {
            StartStoreTrigger();
        }
        else { 
            StopStoreTrigger();
        }
	}

    private void OnDisable()
    {
        ps.Stop();
        gameObject.SetActive(false);
    }

    public void StopStoreTrigger()
    {
        OnDisable();
    }

    public void StartStoreTrigger()
    {        
        gameObject.SetActive(true);
        ps.Play();
    }

    public void EmitExtra(int amount, Color c)
    {
        Debug.Log("Extra emission");
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.startColor = c;
        ps.Emit(emitParams, amount);
    }
}
