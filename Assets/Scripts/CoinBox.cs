using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBox : MonoBehaviour {

    PlayerController player;
    Destructable destructable;


    [SerializeField]
    int minCoin;

    [SerializeField]
    int maxCoin;

    Material mat;
    private void Start()
    {
        destructable = GetComponent<Destructable>();
        player = PlayerController.instance;

        MeshRenderer mr = GetComponent<MeshRenderer>();
        mat = Instantiate(mr.material);
        mr.material = mat;
    }

    [SerializeField]
    Color healthyColor = Color.white;

    [SerializeField]
    Color deadColor = Color.black;

    void Crack()
    {
        mat.color = Color.Lerp(deadColor, healthyColor, destructable.PartialHealth);
        Debug.Log(destructable.PartialHealth);
        //Animate to cracked state if not there
    }

    void Break()
    {
        //Animate to break
        //Spawn coins

        //Temporary
        player.Stats.Coin += Random.Range(minCoin, maxCoin);
        mat.color = deadColor;
        transform.localScale = new Vector3(1, 0.1f, 1);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 1);
    }
}
