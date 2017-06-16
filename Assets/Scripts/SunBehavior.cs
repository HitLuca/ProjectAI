using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunBehavior : MonoBehaviour{
    public float orbitRadius;
    public float angleStep;

    public float sunAngle;

    public bool isOrbiting;
    
    void FixedUpdate () {
        if (isOrbiting)
        {
            sunAngle = (sunAngle + angleStep) % (2 * Mathf.PI);
            if (sunAngle > 2 && sunAngle < 2* Mathf.PI - 2)
            {
                sunAngle = 2 * Mathf.PI - 2;
            }
            Vector3 newPosition = Vector3.zero;
            newPosition.y = Mathf.Cos(sunAngle) * orbitRadius;
            newPosition.z = Mathf.Sin(sunAngle) * orbitRadius;
            transform.position = newPosition;
            transform.LookAt(Vector3.zero);
        }
    }
}
