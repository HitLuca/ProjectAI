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
        GameObject clone = (GameObject)Instantiate(targetObject, coords, Quaternion.identity);
        clone.transform.position = coords;
    }

    public void DestroyObject(GameObject targetObject) {
        Destroy(targetObject);
    }
}
