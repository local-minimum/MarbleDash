﻿using System.Collections;
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
            if (PlayerRunData.stats.InStore)
            {
                Store.instance.RespawnPlayer();
            }
            else
            {
                if (PlayerRunData.stats.holeMode == PlayModeHoles.InstaKill)
                {
                    KillPlayer(other);
                } else
                {
                    PlayerRunData.stats.Health -= PlayerRunData.stats.holeDamage;
                    if (PlayerRunData.stats.Health <= 0)
                    {
                        KillPlayer(other);
                    } else
                    {
                        Level.instance.DropBall();
                    }
                }
            }
        }
    }

    void KillPlayer(Collider other)
    {
        other.GetComponent<PlayerController>().KillReset(deathMessages[Random.Range(0, deathMessages.Length)]);
    }
}
