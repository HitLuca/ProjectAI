using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunBehavior : MonoBehaviour {
    public float orbitRadius;
    public float angleStep;

    public float angle;

    public bool isOrbiting;

    void FixedUpdate () {
        if (isOrbiting)
        {
            angle += angleStep;
            Vector3 newPosition = Vector3.zero;
            newPosition.y = Mathf.Cos(angle) * orbitRadius;
            newPosition.z = Mathf.Sin(angle) * orbitRadius;
            this.transform.position = newPosition;
            this.transform.LookAt(Vector3.zero);
        }
    }
}
