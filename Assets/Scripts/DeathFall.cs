using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFall : MonoBehaviour {

    [SerializeField]
    Level level;

    int playerLayer;

    private void Start()
    {
        playerLayer = LayerMask.NameToLayer("player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            level.Generate();
            level.Implement();
        }
    }
}
