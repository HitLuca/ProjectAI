using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class ActionController : MonoBehaviour {
	public bool usingVR;

    WorldController WorldControllerScript;
	Camera camera;
	Canvas canvas;

	public GameObject[] cubes;
    CanvasData canvasData;
    
	GameObject activeCube;

    bool initialized = false;

    void Start()
    {
        WorldControllerScript = GameObject.Find("WorldController").GetComponent<WorldController>();
		if (usingVR) {
			camera = FindChild (this.transform, "CenterEyeAnchor").GetComponent<Camera> ();
		} else {
			camera = FindChild (this.transform, "FirstPersonCharacter").GetComponent<Camera> ();
		}
		canvas = FindChild(camera.transform, "Canvas").GetComponent<Canvas>();
		SetCanvasActiveCube (canvasData.activeCubeIndex);
        activeCube = cubes[canvasData.activeCubeIndex];
        UpdateScore(canvasData.score);
        initialized = true;
        if (usingVR)
        {
            WorldControllerScript.VRIsReady();
        } else
        {
            WorldControllerScript.FPSIsReady();
        }
    }

    void Update()
    {
		if (Input.GetButtonDown("Place_Block")) {
            PlaceBlock();
		}
		if (Input.GetButtonDown("Destroy_Block")) {
            DestroyBlock();
		}
		if (Input.GetButtonDown("Cycle_Blocks")) {
			LoopActiveCube ();
		}
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
        canvasData.score = score;
        Text scoreText = canvas.transform.Find("ScoreValueText").GetComponent<Text>();
        scoreText.text = canvasData.score.ToString();
    }

    public void UpdateRequestText(string text)
    {
        Text requestText = canvas.transform.Find("RequestText").GetComponent<Text>();
        requestText.text = text;
    }

    public void SetCanvasData(CanvasData canvasData)
    {
        this.canvasData = canvasData;
    }
}
