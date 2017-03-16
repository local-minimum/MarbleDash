using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraMode {Static, Tracking};

public class ModalCamera : MonoBehaviour {

    [SerializeField]
    Destructable player;

    [SerializeField]
    Vector3 staticPosition;

    [SerializeField]
    Vector3 dynamicZeroVelocityOffset;

    [SerializeField]
    Vector3 dynamicMaxVelocityOffset;

    [SerializeField]
    CameraMode camMode = CameraMode.Static;

    [SerializeField, Range(0, 1)]
    float attack = 0.5f;

    [SerializeField]
    KeyCode toggleKey = KeyCode.Tab;

    Vector3 playerPos = Vector3.zero;

	void Update () {
        if (Input.GetKeyDown(toggleKey))
        {
            switch (camMode)
            {
                case CameraMode.Static:
                    camMode = CameraMode.Tracking;
                    break;
                case CameraMode.Tracking:
                    camMode = CameraMode.Static;
                    break;
            }
        }

		if (camMode == CameraMode.Static)
        {
            if ((transform.position - staticPosition).sqrMagnitude > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, staticPosition, attack);
            } else
            {
                transform.position = staticPosition;
            }
        } else
        {
            Vector3 targetOffset = Vector3.Lerp(dynamicZeroVelocityOffset, dynamicMaxVelocityOffset, player.VelocityEffect);
            Vector3 refPos = Vector3.Lerp(playerPos, player.transform.position, attack);
            transform.position = Vector3.Lerp(transform.position, targetOffset + refPos, attack);
            playerPos = player.transform.position;
        }
	}
}
