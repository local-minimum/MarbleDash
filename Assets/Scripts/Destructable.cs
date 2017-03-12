using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    [SerializeField]
    int maxHealth;

    int health;

    public int Health {
        get
        {
            return health;
        }
    }

    public float PartialHealth
    {
        get
        {
            return health / (float) maxHealth;
        }
    }


    Rigidbody rb;

    [SerializeField]
    float velocityForcePower = 1;

    [SerializeField]
    MonoBehaviour controller;

    [SerializeField]
    string hitMessage;

    [SerializeField]
    string destroyMessage;

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
        health = maxHealth;
    }

    public void Hurt(int points)
    {
        health -= points;
        if (health < 1)
        {
            health = 0;
            controller.SendMessage(destroyMessage);
        } else
        {
            controller.SendMessage(hitMessage);
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
