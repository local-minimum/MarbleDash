using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VelForceUI : MonoBehaviour {

    [SerializeField]
    Destructable playerDestructable;

    Text tField;
    
	void Start () {
        tField = GetComponent<Text>();	
	}
	
	void Update () {
        int velForce = playerDestructable.GetVelocityForce();
        int maxVelForce = playerDestructable.MaxVelocityForce;

        string s = "";
        for (int i = 0; i<maxVelForce; i++)
        {
            if (i < velForce)
            {
                s += "#";
            } else
            {
                s += " ";
            }
        }
        tField.text = string.Format("[{0}]", s);
	}
}
