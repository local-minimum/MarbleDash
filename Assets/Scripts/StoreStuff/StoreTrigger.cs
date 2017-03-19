using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreTrigger : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer && gameObject.activeSelf)
        {
            StoreSwapper.instance.EnterStore();
        }
    }

    int playerLayer;
    ParticleSystem ps;

    void Start () {
        playerLayer = LayerMask.NameToLayer("player");
        ps = GetComponent<ParticleSystem>();
        StopStoreTrigger();
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
}
