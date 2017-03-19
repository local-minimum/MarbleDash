using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreItemTrigger : MonoBehaviour {

    [HideInInspector]
    public int itemIndex = -1;

    int playerLayer;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            Store.Display(itemIndex);
        }
    }
}
