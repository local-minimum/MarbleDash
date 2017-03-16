﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBox : MonoBehaviour {

    PlayerController player;
    Destructable destructable;    

    [SerializeField]
    int minCoin;

    [SerializeField]
    int maxCoin;

    [SerializeField]
    Material[] MaterialSequencePrefabs;

    Material[] mats = new Material[3];

    MeshRenderer mr;

    private void Awake()
    {
        destructable = GetComponent<Destructable>();
        player = PlayerController.instance;
        mr = GetComponent<MeshRenderer>();
        for (int i = 0; i < MaterialSequencePrefabs.Length; i++)
        {
            mats[i] = Instantiate(MaterialSequencePrefabs[i]);
        }
        mr.material = mats[0];

    }

    GridPos pos;

    public void SetPosition(GridPos pos)
    {
        transform.localPosition = BoardGrid.instance.GetLocalPosition(pos);
        mr.material = mats[0];

        this.pos = pos;
        if (!BoardGrid.instance.HasOccupancy(pos, Occupancy.Obstacle)) {
            BoardGrid.instance.Occupy(pos, Occupancy.Obstacle);
        }
    }

    [SerializeField]
    Color healthyColor = Color.white;

    [SerializeField]
    Color deadColor = Color.black;

    void Crack()
    {
        if (destructable.PartialHealth < 0.4f)
        {
            mr.material = mats[2];
        } else if (destructable.PartialHealth < 0.75f)
        {
            mr.material = mats[1];
        }

        mr.material.color = Color.Lerp(deadColor, healthyColor, destructable.PartialHealth);
        //Debug.Log(destructable.PartialHealth);
        //Animate to cracked state if not there
    }

    void Break()
    {
        //Animate to break
        //Spawn coins

        //Temporary
        int coin = Random.Range(minCoin, maxCoin);
        if (coin > 0)
        {
            player.Stats.Coin += coin;
        }
        mr.material = mats[2];
        mr.material.color = deadColor;

        BoardGrid.instance.Free(pos, Occupancy.Obstacle);

        transform.localScale = new Vector3(
            transform.localScale.x * 1.1f,
            transform.localScale.y * 0.02f,
            transform.localScale.z * 1.1f);

        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 1);
    }
}