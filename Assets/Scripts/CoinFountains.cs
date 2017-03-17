using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinFountains : MonoBehaviour {


    static CoinFountains _instance;

    public static CoinFountains instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CoinFountains>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        } else
        {
            Destroy(gameObject);
        }

    }

    ParticleSystem ps;
    Quaternion localRot;

    void Start () {
        ps = GetComponent<ParticleSystem>();
        localRot = transform.localRotation;
	}

    [SerializeField]
    Vector3 placementOffset = Vector3.zero;

    [SerializeField]
    Transform innerBoard;

    public void ShowerMe(Transform t)
    {
        transform.SetParent(t, false);
        transform.localPosition = placementOffset;
        transform.SetParent(innerBoard, true);
        transform.localRotation = localRot;
        transform.localScale = Vector3.one;
        ps.Play();
    }
}
