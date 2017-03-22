using UnityEngine;

public delegate void HealthChange();

public class Destructable : MonoBehaviour {

    public event HealthChange OnHealthChange;

    [SerializeField, HideInInspector]
    int maxHealth;

    int health;

    virtual public int Health {
        get
        {
            return health;
        }

        protected set
        {
            health = value;
        }
    }

    virtual public float PartialHealth
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
    public float VelocityEffect
    {
        get
        {
            return velForce / (float)MaxVelocityForce;
        }
    }

    public int GetVelocityForce()
    {
        return velForce;
    }

    public int MaxVelocityForce = 10;

    int recordDepth = 10;
    float[] speedRecord;
    int recordIndex = -1;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        speedRecord = new float[20];
        SetInitialHealthOnStart();
    }

    protected virtual void SetInitialHealthOnStart()
    {
        health = maxHealth;
        if (OnHealthChange != null)
        {
            OnHealthChange();
        }

    }

    public void Hurt(int points)
    {
        points = Mathf.Min(points, Health);
        Health -= points;
        if (Health <= 0)
        {
            Health = 0;
            controller.SendMessage(destroyMessage, points);
        } else
        {
            controller.SendMessage(hitMessage, points);
        }

        if (OnHealthChange != null)
        {
            OnHealthChange();
        }
    }

    public void Emit()
    {
        if (OnHealthChange != null)
        {
            OnHealthChange();
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
