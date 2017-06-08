using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void PlaceObject(GameObject targetObject, Vector3 coords)
    {
		if(emptySpace(targetObject.transform.position)) {
	        GameObject clone = (GameObject)Instantiate(targetObject, coords, Quaternion.identity);
	        clone.transform.position = coords;
		} else {
			Debug.Log("Space occupied");
		}
    }

    public void DestroyObject(GameObject targetObject) {
        Destroy(targetObject);
    }

	bool emptySpace(Vector3 position) {
		return Physics.CheckSphere (position, 0.9f);
	}
}
