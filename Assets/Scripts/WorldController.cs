using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class WorldController : MonoBehaviour {
    public GameObject FPSController;
    public GameObject VRController;

    ActionController FPSScript;
    ActionController VRScript;
    bool usingVR;
    public int GuiBlocksNumber;

    void Start() {
        usingVR = VRSettings.loadedDeviceName == "Oculus";
        FPSScript = FPSController.GetComponent<ActionController>();
        VRScript = VRController.GetComponent<ActionController>();

        CanvasData canvasData = new CanvasData(GuiBlocksNumber);

        FPSScript.SetCanvasData(canvasData);
        VRScript.SetCanvasData(canvasData);

        if (!usingVR)
        {
            EnableFPSController();
        }
        else
        {
            EnableVRController();
        }
    }
    
    void Update() {
        if (Input.GetButtonDown("ChangePlayMode"))
        {
            if (usingVR)
            {
                EnableFPSController();
            } else
            {
                EnableVRController();
            }
        }
    }

    public void PlaceObject(GameObject targetObject, Vector3 coords)
    {
		if(EmptySpace(targetObject.transform.position)) {
	        GameObject clone = (GameObject)Instantiate(targetObject, coords, Quaternion.identity);
	        clone.transform.position = coords;
		} else {
			Debug.Log("Space occupied");
		}
    }

    public void DestroyObject(GameObject targetObject) {
        Destroy(targetObject);
    }

	bool EmptySpace(Vector3 position) {
		return Physics.CheckSphere (position, 0.9f);
	}

    public bool UsingVR()
    {
        return usingVR;
    }

    void EnableFPSController()
    {
        FPSController.transform.position = VRController.transform.position;
        VRController.SetActive(false);
        FPSController.SetActive(true);
        VRSettings.LoadDeviceByName("None");
        usingVR = false;
    }

    void EnableVRController()
    {
        VRController.transform.position = FPSController.transform.position;
        FPSController.SetActive(false);
        VRController.SetActive(true);
        VRSettings.LoadDeviceByName("Oculus");
        usingVR = true;
    }

    public void UpdatePlayerScore(int score)
    {
        if (usingVR)
        {
            VRScript.UpdateScore(score);
        }
        else
        {
            FPSScript.UpdateScore(score);
        }
    }

    public void UpdatePlayerRequestText(string text)
    {
        if (usingVR)
        {
            VRScript.UpdateRequestText(text);
        }
        else
        {
            FPSScript.UpdateRequestText(text);
        }
    }
}
