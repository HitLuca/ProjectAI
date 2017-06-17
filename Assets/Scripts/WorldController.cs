using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

public class WorldController : MonoBehaviour {
    [Header("Player controllers")]
    public GameObject FPSController;
    public GameObject VRController;

    [Header("Blocks used in game")]
    public int blocksNumber;

    [Header("Tree spawning")]
    public bool spawnTrees;
    [Range(10, 250)]
    public int spawnSquareLength;
    [Range(0f, 1f)]
    public float spawnPercentage;
    public GameObject tree;

    ActionController FPSActionControllerScript;
    ActionController VRActionControllerScript;

    FirstPersonController FPSControllerScript;
    OVRPlayerController VRControllerScript;

    bool usingVR;
    bool FPSReady = false;
    bool VRReady = false;

    void Start() {
        usingVR = VRSettings.loadedDeviceName == "Oculus";
        FPSActionControllerScript = FPSController.GetComponent<ActionController>();
        VRActionControllerScript = VRController.GetComponent<ActionController>();

        FPSControllerScript = FPSController.GetComponent<FirstPersonController>();
        VRControllerScript = VRController.GetComponent<OVRPlayerController>();

        CanvasData canvasData = new CanvasData(blocksNumber);

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


        if(spawnTrees)
        {
            SpawnTrees();
        }
    }
    
    void Update() {
        if (Input.GetButtonDown("Change_Play_Mode"))
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
            VRActionControllerScript.UpdateScore(score);
        }
        else
        {
            FPSActionControllerScript.UpdateScore(score);
        }
    }

    public void UpdatePlayerRequestText(string text)
    {
        if (usingVR)
        {
            VRActionControllerScript.UpdateRequestText(text);
        }
        else
        {
            FPSActionControllerScript.UpdateRequestText(text);
        }
    }

    public bool IsPlayerCrouched()
    {
        if (usingVR)
        {
            return VRControllerScript.IsCrouched();
        }
        else
        {
            return FPSControllerScript.IsCrouched();
        }
    }

    public void TogglePlayerCrouch()
    {
        if (usingVR)
        {
            VRControllerScript.ToggleCrouch();
        }
        else
        {
            FPSControllerScript.ToggleCrouch();
        }
    }
    
    public void AnimateTextPosition()
    {
        if (usingVR)
        {
            VRActionControllerScript.resetText();
        }
        else
        {
            FPSActionControllerScript.resetText();
        }
    }

    void SpawnTrees() {
        int halfSide = Mathf.RoundToInt(spawnSquareLength / 2.0f);
        for (int x = -halfSide; x < halfSide; x++)
        {
            for (int z = -halfSide; z < halfSide; z++)
            {
                if (Random.Range(0f, 1f) < spawnPercentage && x!=0 && z!=0)
                {
                    Vector3 coords = new Vector3(x, 0, z);
                    PlaceObject(tree, coords);
                }
            }
        }
    }
}
