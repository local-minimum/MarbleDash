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

    int velForce;

    public int GetVelocityForce()
    {
        return velForce;
    }

    public int MaxVelocityForce = 10;

    int recordDepth = 20;
    float[] speedRecord;
    int recordIndex = -1;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        speedRecord = new float[20];
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("walls"))
        {
            Debug.Log(GetVelocityForce());
        }
    }

    private void Update()
    {
        if (rb)
        {
            recordIndex++;
            recordIndex %= recordDepth;
            speedRecord[recordIndex] = rb.velocity.magnitude;

            float vbar = 0;
            for (int i=0; i<recordDepth; i++)
            {
                vbar += speedRecord[i];
            }
            velForce = Mathf.Clamp(Mathf.FloorToInt(vbar/recordDepth), 0, MaxVelocityForce);

        }
    }
}
