using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class WorldController : MonoBehaviour {
    public GameObject FPSController;
    public GameObject VRController;

    ActionController FPSActionControllerScript;
    ActionController VRActionControllerScript;

    FirstPersonController FPSControllerScript;
    OVRPlayerController VRControllerScript;

    bool usingVR;
    public int GuiBlocksNumber;

    bool FPSReady = false;
    bool VRReady = false;

    void Start() {
        usingVR = VRSettings.loadedDeviceName == "Oculus";
        FPSActionControllerScript = FPSController.GetComponent<ActionController>();
        VRActionControllerScript = VRController.GetComponent<ActionController>();

        FPSControllerScript = FPSController.GetComponent<FirstPersonController>();
        VRControllerScript = VRController.GetComponent<OVRPlayerController>();

        CanvasData canvasData = new CanvasData(GuiBlocksNumber);

        FPSActionControllerScript.SetCanvasData(canvasData);
        VRActionControllerScript.SetCanvasData(canvasData);

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
        FPSController.SetActive(true);
        VRSettings.LoadDeviceByName("None");
        VRController.SetActive(false);
        usingVR = false;
    }

    void EnableVRController()
    {
        VRController.transform.position = FPSController.transform.position;
        VRController.SetActive(true);
        VRSettings.LoadDeviceByName("Oculus");
        FPSController.SetActive(false);
        usingVR = true;
    }

    public void UpdatePlayerScore(int score)
    {
        if (usingVR)
        {
            if (VRReady)
            {
                VRActionControllerScript.UpdateScore(score);
            }
        }
        else
        {
            if (FPSReady)
            {
                FPSActionControllerScript.UpdateScore(score);
            }
        }
    }

    public void UpdatePlayerRequestText(string text)
    {
        if (usingVR && VRReady)
        {
            VRActionControllerScript.UpdateRequestText(text);
        }
        else if (!usingVR && FPSReady)
        {
            FPSActionControllerScript.UpdateRequestText(text);
        }
    }

    public void FPSIsReady()
    {
        FPSReady = true;
    }

    public void VRIsReady()
    {
        VRReady = true;
    }

    public bool IsPlayerCrouched()
    {
        if (usingVR && VRReady)
        {
            return VRControllerScript.IsCrouched();
        }
        else if (!usingVR && FPSReady)
        {
            return FPSControllerScript.IsCrouched();
        }
        else return false;
    }

    public void TogglePlayerCrouch()
    {
        if (usingVR && VRReady)
        {
            VRControllerScript.ToggleCrouch();
        }
        else if (!usingVR && FPSReady)
        {
            FPSControllerScript.ToggleCrouch();
        }
    }
}
