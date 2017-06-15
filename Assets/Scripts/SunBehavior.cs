using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunBehavior : MonoBehaviour {
    public float orbitRadius;
    public float angleStep;

    private float angle;

    private void Start()
    {
        angle = -1;
    }

    void FixedUpdate () {
        angle += angleStep;
        Vector3 newPosition = Vector3.zero;
        newPosition.y = Mathf.Cos(angle) * orbitRadius;
        newPosition.z = Mathf.Sin(angle) * orbitRadius;
        this.transform.position = newPosition;
        this.transform.LookAt(Vector3.zero);
    }
}
