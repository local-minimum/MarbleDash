using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [SerializeField]
    BoardController board;

    Rigidbody rb;

    bool grounded = false;

	void Start () {
        rb = GetComponent<Rigidbody>();
        groundLayer = LayerMask.NameToLayer("ground");
    }
	
	void Update () {
        if (grounded)
        {
            rb.AddForce(board.Slope);
        }
    }

    int groundLayer;

    private void OnCollisionEnter(Collision collision)
    {
        grounded = groundLayer == collision.gameObject.layer;
    }

}
