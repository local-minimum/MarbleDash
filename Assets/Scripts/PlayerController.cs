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
	}
	
	void Update () {
        if (grounded)
        {
            rb.AddForce(board.Slope);
        }
    }

    int groundLayer = LayerMask.NameToLayer("ground");

    private void OnCollisionEnter(Collision collision)
    {
        grounded = groundLayer == collision.gameObject.layer;
    }

}
