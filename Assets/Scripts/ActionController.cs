using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ActionController : MonoBehaviour {
    WorldController WorldControllerScript;
	Camera camera;
	Canvas canvas;
	public GameObject[] cubes;
	int activeCubeIndex = 0;
	GameObject activeCube;

    void Start()
    {
        WorldControllerScript = GameObject.Find("WorldController").GetComponent<WorldController>();
		camera = transform.Find ("OVRCameraRig/TrackingSpace/CenterEyeAnchor").GetComponent<Camera>();
		canvas = camera.transform.Find("Canvas").GetComponent<Canvas>();
		activeCube = cubes [0];
		SetCanvasActiveCube (0);
    }
    void Update()
    {
		if (Input.GetMouseButtonDown (0) || OVRInput.GetDown(OVRInput.Button.Three)) {
			Debug.Log ("Click");
			RaycastHit hit;
			GameObject centerAnchor = GameObject.Find ("CenterEyeAnchor");
			Vector3 forward = centerAnchor.transform.TransformDirection (Vector3.forward);

			if (Physics.Raycast (centerAnchor.transform.position, forward, out hit)) {
				Transform objectHit = hit.transform;
				if (isCubeSide (objectHit)) {
					WorldControllerScript.DestroyObject (objectHit.parent.gameObject);
				} else {
					Debug.Log ("Not deletable!");
				}
			}
		}
		if (Input.GetMouseButtonDown (1) || OVRInput.GetDown(OVRInput.Button.Two)) {
			RaycastHit hit;
			GameObject centerAnchor = GameObject.Find ("CenterEyeAnchor");
			Vector3 forward = centerAnchor.transform.TransformDirection (Vector3.forward);

			if (Physics.Raycast (centerAnchor.transform.position, forward, out hit)) {
				Transform objectHit = hit.transform;
				if (isCubeSide (objectHit)) {
					Vector3 newCoordinates = CalculateNewCoordinates (objectHit.parent.transform.position, objectHit.gameObject.name);
					WorldControllerScript.PlaceObject (activeCube, newCoordinates);
				}
			}
		}

		if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)) {
			LoopActiveCube ();
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

	void SetCanvasActiveCube (int index) {
		canvas.transform.Find("ActiveCubes").transform.GetChild(index).GetComponent<Outline> ().enabled = true;
	}

	void SetCanvasUnactiveCube (int index) {
		canvas.transform.Find("ActiveCubes").transform.GetChild(index).GetComponent<Outline> ().enabled = false;
	}

	void LoopActiveCube () {
		SetCanvasUnactiveCube (activeCubeIndex);
		activeCubeIndex = (activeCubeIndex + 1) % cubes.Length;
		activeCube = cubes [activeCubeIndex];
		SetCanvasActiveCube (activeCubeIndex);
	}
}
