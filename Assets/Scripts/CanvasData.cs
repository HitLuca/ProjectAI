using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasData : MonoBehaviour {
    public int score = 0;
    public int activeCubeIndex = 0;
    public int cubesNumber;
    
    public CanvasData(int cubesNumber)
    {
        this.cubesNumber = cubesNumber;
    }
}
