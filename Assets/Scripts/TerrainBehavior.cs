using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBehavior : MonoBehaviour {
    [Header("Spawning")]
    [Range(10, 250)]
    public int spawnSquareLength;

    [Header("Tree spawning")]
    public bool spawnTrees;
    [Range(0f, 1f)]
    public float treesSpawnPercentage;
    public GameObject[] trees;

    [Header("Grass spawning")]
    public bool spawnGrass;
    [Range(0f, 1f)]
    public float grassSspawnPercentage;
    public GameObject[] grass;

    [Header("Details spawning")]
    public bool spawnDetails;
    [Range(0f, 1f)]
    public float detailsSpawnPercentage;
    public GameObject[] details;

    WorldController worldControllerScript;

    private void Start()
    {
        worldControllerScript = GameObject.Find("WorldController").GetComponent<WorldController>();

        if (spawnTrees)
        {
            Spawn(trees, treesSpawnPercentage);
        }
        if (spawnGrass)
        {
            Spawn(grass, grassSspawnPercentage);
        }
        if (spawnDetails)
        {
            Spawn(details, detailsSpawnPercentage);
        }
    }
    void Spawn(GameObject[] objects, float spawnPercentage)
    {
        int halfSide = Mathf.RoundToInt(spawnSquareLength / 2.0f);
        for (int x = -halfSide; x < halfSide; x++)
        {
            for (int z = -halfSide; z < halfSide; z++)
            {
                if (Random.Range(0f, 1f) < spawnPercentage && x != 0 && z != 0)
                {
                    Vector3 coords = new Vector3(x, 0, z);
                    worldControllerScript.PlaceObject(objects[Random.Range(0, objects.Length)], coords);
                }
            }
        }
    }
}
