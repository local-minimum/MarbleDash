using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFall : MonoBehaviour {

    [SerializeField]
    Level level;

    int playerLayer;

    [SerializeField]
    string[] deathMessages;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            other.GetComponent<PlayerController>().KillReset(deathMessages[Random.Range(0, deathMessages.Length)]);
            StoreSwapper.instance.HideAllStores();
            level.Generate();
            level.Implement();
        }
    }
}
