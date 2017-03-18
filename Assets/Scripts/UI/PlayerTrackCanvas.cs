using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrackCanvas : MonoBehaviour {

    [SerializeField]
    Transform player;

    Vector3 offset;

	void Start () {
        offset = transform.position - player.position;		
	}


    private void LateUpdate()
    {
        transform.position = player.position + offset;
    }
}
