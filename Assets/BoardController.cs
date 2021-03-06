﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LocalMinimum;

public class BoardController : Singleton<BoardController> {

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

    bool useGyro = false;
    KeyCode gyroToggle = KeyCode.G;
    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            useGyro = true;
        }
    }

    public void Balance()
    {
        innerRotation = 0;
        outerRotation = 0;
    }

    public Vector2 CurrentTilt
    {
        get
        {
            return new Vector2(innerRotation, outerRotation);
        }
    }

    Vector2 delayedTiltIndicator = Vector2.zero;

    public Vector2 DelayedTilt
    {
        get
        {
            return delayedTiltIndicator;
        }
    }

    [SerializeField, Range(0, 2)]
    float delayTiltWeight = 1;

    void Update() {

        if (Input.GetKeyDown(gyroToggle))
        {
            if (useGyro)
            {
                useGyro = false;
            } else
            {
                useGyro = SystemInfo.supportsGyroscope;
            }
        }

        Vector2 target;
        if (useGyro)
        {
            target = TiltControl();
        } else
        {
            target = ClassicControl();
        }

        innerRotation = Mathf.Lerp(target.y, innerRotation, delay);
        outerRotation = Mathf.Lerp(target.x, outerRotation, delay);

        outerControl.localEulerAngles = outerEulerAxis * outerRotation;
        innerControl.localEulerAngles = innerEulerAxis * innerRotation;

        delayedTiltIndicator = (delayTiltWeight * delayedTiltIndicator + CurrentTilt * Time.deltaTime) / (delayTiltWeight + Time.deltaTime);
    }

    Vector2 ClassicControl()
    {
        float innerTarget = Input.GetAxis("Horizontal");
        float outerTarget = Input.GetAxis("Vertical");
        return new Vector2(outerTarget, innerTarget) * rotationMagnitude;
    }

    Vector2 TiltControl ()
    {
        Gyroscope gyro = Input.gyro;
        Vector3 euler = gyro.attitude.eulerAngles;

        float x = euler.x;
        if (x > 180)
        {
            x -= 180;
        }

        float y = euler.y;
        if (y > 180)
        {
            y -= 180;
        }

        x = Mathf.Clamp(x, -rotationMagnitude, rotationMagnitude);
        y = Mathf.Clamp(y, -rotationMagnitude, rotationMagnitude);

        return new Vector2(x, y);
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
