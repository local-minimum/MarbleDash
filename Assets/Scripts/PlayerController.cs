using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    static PlayerController _instance;

    public static PlayerController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerController>();
            }
            return _instance;
        }
    }

    [SerializeField]
    PlayerStats playerStats = new PlayerStats();

    public PlayerStats Stats
    {
        get
        {
            return playerStats;
        }
    }

    [SerializeField]
    BoardController board;

    Rigidbody rb;

    bool grounded = false;

    Destructable destructable;

    private void Awake()
    {
        if (_instance == this || _instance == null)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start () {
        rb = GetComponent<Rigidbody>();
        groundLayer = LayerMask.NameToLayer("ground");
        destructableLayer = LayerMask.NameToLayer("destructables");
        destructable = GetComponent<Destructable>();
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
    int destructableLayer;

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer == destructableLayer)
        {
            Destructable otherDest = collision.gameObject.GetComponent<Destructable>();
            otherDest.Hurt(destructable.GetVelocityForce());
        }
    }

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

    public void HurtMe()
    {

    }

    public void KillMe()
    {

    }
}
