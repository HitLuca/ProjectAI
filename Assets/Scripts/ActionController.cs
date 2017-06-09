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
	int activeCubeIndex = 0;
	GameObject activeCube;

    void Start()
    {
        WorldControllerScript = GameObject.Find("WorldController").GetComponent<WorldController>();
		if (usingVR) {
			camera = FindChild (this.transform, "CenterEyeAnchor").GetComponent<Camera> ();
		} else {
			camera = FindChild (this.transform, "FirstPersonCharacter").GetComponent<Camera> ();
		}
		canvas = FindChild(camera.transform, "Canvas").GetComponent<Canvas>();
		activeCube = cubes [0];
		SetCanvasActiveCube (0);
    }
    void Update()
    {
		if (Input.GetButtonDown("PlaceBlock")) {
			RaycastHit hit;
			Vector3 forward = camera.transform.TransformDirection (Vector3.forward);

			if (Physics.Raycast (camera.transform.position, forward, out hit)) {
				Transform objectHit = hit.transform;

                if (isCubeSide (objectHit)) {
					UpdateScore (objectHit.parent.name);
					WorldControllerScript.DestroyObject (objectHit.parent.gameObject);
				} else {
					Debug.Log ("Not deletable!");
				}
			}
		}
		if (Input.GetButtonDown("DestroyBlock")) {
			RaycastHit hit;
			Vector3 forward = camera.transform.TransformDirection (Vector3.forward * 10);

			if (Physics.Raycast (camera.transform.position, forward, out hit)) {
				Transform objectHit = hit.transform;

                if (isCubeSide (objectHit)) {
                    Debug.Log("hitted cube side");
					Vector3 newCoordinates = CalculateNewCoordinates (objectHit.parent.transform.position, objectHit.gameObject.name);
					WorldControllerScript.PlaceObject (activeCube, newCoordinates);
				} else {
					Vector3 newCoordinates = CalculateWorldCoordinates (hit.point);
                    WorldControllerScript.PlaceObject(activeCube, newCoordinates);
                }
			}
		}
		if (Input.GetButtonDown("CycleBlocks")) {
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

	void UpdateScore(string name) {
		int score = GetScore ();

		if (name.Contains ("DirtCube")) {
			score += 1;
		} else if (name.Contains ("GrassCube")) {
			score += 5;
		} else if (name.Contains ("StoneCube")) {
			score += 10;
		}
		Text scoreText = canvas.transform.Find ("ScoreValueText").GetComponent<Text> ();
		scoreText.text = score.ToString();
	}

	int GetScore() {
		Text scoreText = canvas.transform.Find ("ScoreValueText").GetComponent<Text> ();
		return int.Parse (scoreText.text);
	}

    public void UpdateText(string text)
    {
        Text requestText = canvas.transform.Find("RequestText").GetComponent<Text>();
        requestText.text = text;
    }
}
