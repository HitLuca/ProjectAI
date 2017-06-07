using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActionController : MonoBehaviour {
    public Camera camera;

    WorldController WorldControllerScript;

    void Start()
    {
        WorldControllerScript = GameObject.Find("WorldController").GetComponent<WorldController>();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(new Vector2(Screen.width /2, Screen.height/2));

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                if (isCubeSide(objectHit))
                {
                    WorldControllerScript.DestroyObject(objectHit.parent.gameObject);
                }
                else
                {
                    Debug.Log("Not deletable!");
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;
                if (isCubeSide(objectHit))
                {
                    Vector3 newCoordinates = CalculateNewCoordinates(objectHit.parent.transform.position, objectHit.gameObject.name);
                    WorldControllerScript.PlaceObject(objectHit.parent.gameObject, newCoordinates);
                }
            }
        }

    }

    bool isCubeSide(Transform o)
    {
        string objectName = o.gameObject.name;

        string[] sides = { "Front", "Back", "Top", "Bottom", "Left", "Right" };
        if (sides.Contains(objectName))
        {
            return true;
        }
        return false;
    }

    Vector3 CalculateNewCoordinates(Vector3 currentPosition, string name)
    {
        Vector3 delta = Vector3.zero;
        switch(name)
        {
            case "Front":
                {
                    delta = new Vector3(0, 0, 1);
                    break;
                }
            case "Back":
                {
                    delta = new Vector3(0, 0, -1);
                    break;
                }
            case "Top":
                {
                    delta = new Vector3(0, 1, 0);
                    break;
                }
            case "Bottom":
                {
                    delta = new Vector3(0, -1, 0);
                    break;
                }
            case "Left":
                {
                    delta = new Vector3(-1, 0, 0);
                    break;
                }
            case "Right":
                {
                    delta = new Vector3(1, 0, 0);
                    break;
                }

        }
        return currentPosition + delta;
    }
}
