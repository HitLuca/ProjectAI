using UnityEngine;
using System.Collections;

public class CrouchController : MonoBehaviour
{
    public float crchSpeed = 3; // crouching speed

    //private CharacterMotor chMotor;
    private Transform tr;
    private float dist; // distance to ground
    private bool crouched = false;

    // Use this for initialization
    void Start()
    {
        tr = transform;
        CharacterController ch = GetComponent<CharacterController>();
        dist = ch.height / 2; // calculate distance to ground
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float vScale = 1.0f;

        if (Input.GetKey("c"))
        {
            vScale = 0.5f;
        }

        //chMotor.movement.maxForwardSpeed = speed; // set max speed
        float ultScale = tr.localScale.y; // crouch/stand up smoothly 

        Vector3 tmpScale = tr.localScale;
        Vector3 tmpPosition = tr.position;

        tmpScale.y = Mathf.Lerp(tr.localScale.y, vScale, 5 * Time.deltaTime);
        tr.localScale = tmpScale;

        tmpPosition.y += dist * (tr.localScale.y - ultScale); // fix vertical position        
        tr.position = tmpPosition;
    }
}
