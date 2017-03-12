using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    [SerializeField]
    float health;

    Rigidbody rb;

    [SerializeField]
    float velocityForcePower = 1;


    float VelocityForce
    {
        get
        {
            if (rb == null)
            {
                return 0;
            }
            return Mathf.Pow(rb.velocity.magnitude, velocityForcePower) * rb.mass;
        }
    }

    public int GetVelocityForce()
    {
        return Mathf.Clamp(Mathf.FloorToInt(VelocityForce), 0, MaxVelocityForce);
    }

    public int MaxVelocityForce = 10;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(LayerMask.LayerToName(collision.gameObject.layer));

        if (collision.gameObject.layer == LayerMask.NameToLayer("walls"))
        {
            Debug.Log(VelocityForce);
        }
    }
}
