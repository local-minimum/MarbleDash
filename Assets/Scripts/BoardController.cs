using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

    [SerializeField]
    Transform outerControl;

    [SerializeField]
    Transform innerControl;

    [SerializeField, Range(0, 15)]
    float rotationMagnitude;

    [SerializeField]
    Vector3 outerEulerAxis = new Vector3(1, 0, 0);

    [SerializeField]
    Vector3 innerEulerAxis = new Vector3(0, 1, 0);

    float innerRotation;

    float outerRotation;

    [SerializeField, Range(0, 1)]
    float delay; 

    public void Balance()
    {
        innerRotation = 0;
        outerRotation = 0;
    }

	void Update () {
        float innerTarget = Input.GetAxis("Horizontal");
        float outerTarget = Input.GetAxis("Vertical");

        innerRotation = Mathf.Lerp(innerTarget * rotationMagnitude, innerRotation, delay);
        outerRotation = Mathf.Lerp(outerTarget * rotationMagnitude, outerRotation, delay);

        outerControl.localEulerAngles = outerEulerAxis * outerRotation;
        innerControl.localEulerAngles = innerEulerAxis * innerRotation;
    }

    public Vector3 Slope
    {
        get
        {

            return new Vector3(outerEulerAxis.y, 0, outerEulerAxis.x) * rotationMagnitude * outerRotation + 
                new Vector3(innerEulerAxis.y, 0, innerEulerAxis.x) * rotationMagnitude * innerRotation;
        }
    }
}
