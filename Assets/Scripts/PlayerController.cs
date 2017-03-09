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

    [SerializeField, Range(0, 5)]
    float forceMultiplier = 0.5f;

	void Update () {
        if (grounded)
        {
            rb.AddForce(board.Slope * forceMultiplier);
        }
    }

    int groundLayer;

    private void OnCollisionStay(Collision collision)
    {
        if (groundLayer == collision.gameObject.layer)
        {
            grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (groundLayer == collision.gameObject.layer)
        {
            grounded = false;
        }
    }

}
