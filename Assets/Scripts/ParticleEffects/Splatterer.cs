using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splatterer : MonoBehaviour {

    [SerializeField]
    Vector3 placementOffset;

    ParticleSystem ps;
    static Splatterer _instance;
    public static Splatterer instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<Splatterer>();
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

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Start () {
        ps = GetComponent<ParticleSystem>();	
	}

    [SerializeField]
    int minSplat=4;

    [SerializeField]
    int maxSplat=10;

    public void SplatMe(Transform t)
    {
        transform.position = t.position;
        transform.localPosition += placementOffset;  
        ps.Emit(Random.Range(minSplat, maxSplat));
    }

    public void CleanupSplatter()
    {
        ps.Clear();
    }
}
