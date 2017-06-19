using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class ActionController : MonoBehaviour {

    const int HIGHEST_TEXT_Y = 180;
    const int LOWEST_TEXT_Y = 50;

    private int textY = HIGHEST_TEXT_Y;
    Timer centeredTextTimer = new Timer();

    public bool usingVR;

    WorldController WorldControllerScript;
	Camera camera;
	Canvas canvas;

	public GameObject[] cubes;
    Dictionary<string, GameObject> joystickImages;
    GameObject activeJoystick;
    string activeAction;

    CanvasData canvasData;
    
	GameObject activeCube;

    bool initialized = false;

    void Start()
    {
        UpdateTextPosition();
        WorldControllerScript = GameObject.Find("WorldController").GetComponent<WorldController>();

        // get camera reference
		if (usingVR) {
			camera = FindChild (this.transform, "CenterEyeAnchor").GetComponent<Camera> ();
		} else {
			camera = FindChild (this.transform, "FirstPersonCharacter").GetComponent<Camera> ();
		}
		canvas = FindChild(camera.transform, "Canvas").GetComponent<Canvas>();

        // set initial parameters
		SetCanvasActiveCube (canvasData.activeCubeIndex);
        activeCube = cubes[canvasData.activeCubeIndex];
        UpdateScore(canvasData.score);

        // setup joystick images dictionary
        joystickImages = new Dictionary<string, GameObject>();
        GameObject temp = canvas.transform.Find("Joysticks").gameObject;
        int joystickImagesCount = temp.transform.childCount;
        for (int i = 0; i < joystickImagesCount; i++)
        {
            GameObject image = temp.transform.GetChild(i).gameObject;
            joystickImages[image.name] = image;
        }
        activeJoystick = null;

        // initialization is finished
        initialized = true;
    }

    void Update()
    {
        centeredTextTimer.UpdateTimer();
        if (Input.GetButtonDown("Place_Block")) {
            PlaceBlock();
		}
		if (Input.GetButtonDown("Destroy_Block")) {
            DestroyBlock();
		}
		if (Input.GetButtonDown("Cycle_Blocks")) {
			LoopActiveCube ();
		}

        UpdateTextPosition();
    }

    private void OnEnable()
    {
        if (initialized)
        {
            UpdateScore(canvasData.score);
            SetCanvasActiveCube(canvasData.activeCubeIndex);
            for (int i = 0; i < cubes.Length; i++)
            {
                if (i != canvasData.activeCubeIndex)
                {
                    SetCanvasInactiveCube(i);
                }
            }
        }
    }

    bool IsCubeSide(Transform o)
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

	void SetCanvasInactiveCube(int index) {
		canvas.transform.Find("ActiveCubes").transform.GetChild(index).GetComponent<Outline> ().enabled = false;
	}

	void LoopActiveCube () {
        SetCanvasInactiveCube(canvasData.activeCubeIndex);
        canvasData.activeCubeIndex = (canvasData.activeCubeIndex + 1) % cubes.Length;
		activeCube = cubes [canvasData.activeCubeIndex];
		SetCanvasActiveCube (canvasData.activeCubeIndex);
	}

	Transform FindChild(Transform parent, string name)
	{
		var result = parent.Find(name);
		if (result != null)
			return result;
		foreach(Transform child in parent)
		{
			result = FindChild(child, name);
			if (result != null)
				return result;
		}
		return null;
	}

	Vector3  CalculateWorldCoordinates (Vector3 worldCoordinates) {
		return new Vector3 (Mathf.Round (worldCoordinates.x + 0.5f), 0, Mathf.Round (worldCoordinates.z));
	}

    int  CalculateScore(string name)
    {
        int score = GetScore();

        if (name.Contains("DirtCube"))
        {
            score += 1;
        }
        else if (name.Contains("GrassCube"))
        {
            score += 3;
        }
        else if (name.Contains("CobblestoneCube"))
        {
            score += 5;
        }
        else if (name.Contains("StoneCube"))
        {
            score += 10;
        }
        return score;
    }

    void DestroyBlock()
    {
        RaycastHit hit;
        Vector3 forward = camera.transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(camera.transform.position, forward, out hit))
        {
            Transform objectHit = hit.transform;

            if (IsCubeSide(objectHit))
            {
                int score = CalculateScore(objectHit.parent.name);
                WorldControllerScript.UpdatePlayerScore(score);

                WorldControllerScript.DestroyObject(objectHit.parent.gameObject);
            }
            else
            {
                Debug.Log("Not deletable!");
            }
        }
    }

    void PlaceBlock()
    {
        RaycastHit hit;
        Vector3 forward = camera.transform.TransformDirection(Vector3.forward * 10);

        if (Physics.Raycast(camera.transform.position, forward, out hit))
        {
            Transform objectHit = hit.transform;

            if (IsCubeSide(objectHit))
            {
                Vector3 newCoordinates = CalculateNewCoordinates(objectHit.parent.transform.position, objectHit.gameObject.name);
                WorldControllerScript.PlaceObject(activeCube, newCoordinates);
            }
            else
            {
                Vector3 newCoordinates = CalculateWorldCoordinates(hit.point);
                WorldControllerScript.PlaceObject(activeCube, newCoordinates);
            }
        }
    }

    int GetScore() {
        return canvasData.score;
	}

    public void UpdateScore(int score)
    {
        try
        {
            canvasData.score = score;
            Text scoreText = canvas.transform.Find("ScoreValueText").GetComponent<Text>();
            scoreText.text = canvasData.score.ToString();
        }
        catch (Exception e) { }
    }

    public void UpdateRequestText(string text)
    {
        try
        {
            Text requestText = canvas.transform.Find("RequestText").GetComponent<Text>();
            requestText.text = text;
        }
        catch (Exception e) { }
    }

    public void SetCanvasData(CanvasData canvasData)
    {
        this.canvasData = canvasData;
    }

    public void UpdateTextPosition()
    {
        
        if (centeredTextTimer.isFinished() && textY < HIGHEST_TEXT_Y)
        {
            Text requestText = canvas.transform.Find("RequestText").GetComponent<Text>();
            
            requestText.transform.localPosition = new Vector3(requestText.transform.localPosition.x, textY, requestText.transform.localPosition.z);
            textY += 5;
        }
    }

    public void ResetText()
    {
        textY = LOWEST_TEXT_Y;
        UpdateTextPosition();
        centeredTextTimer.StartTimer(1.5f);
    }

    public void DisplayJoystick(string actionName)
    {
        try
        {
            if (!String.Equals(activeAction, actionName))
            {
                if (activeJoystick != null)
                {
                    activeJoystick.GetComponent<Image>().enabled = false;
                }
                switch (actionName)
                {
                    case "Jump":
                        joystickImages["XboxControllerA"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerA"];
                        break;
                    case "Destroy  Block":
                        joystickImages["XboxControllerB"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerB"];
                        break;
                    case "Place  Block":
                        joystickImages["XboxControllerX"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerX"];
                        break;
                    case "Crouch":
                        joystickImages["XboxControllerRT"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerRT"];
                        break;
                    case "Stand  Up":
                        joystickImages["XboxControllerRT"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerRT"];
                        break;
                    case "Walk":
                        joystickImages["XboxControllerFW"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerFW"];
                        break;
                    case "Turn  Left":
                        joystickImages["XboxControllerTL"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerTL"];
                        break;
                    case "Turn  Right":
                        joystickImages["XboxControllerTR"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerTR"];
                        break;
                    case "Run":
                        joystickImages["XboxControllerSP"].GetComponent<Image>().enabled = true;
                        activeJoystick = joystickImages["XboxControllerSP"];
                        break;
                    default:
                        Debug.LogError("Action " + actionName + " is not mapped to any joystick image!");
                        break;
                }
                activeAction = actionName;
            }
        }
        catch (Exception e) { }
    }

    public void HideJoysticks()
    {
        try
        {
            activeJoystick.GetComponent<Image>().enabled = false;
            activeJoystick = null;
            activeAction = null;
        }
        catch (Exception e) { }
    }
}
