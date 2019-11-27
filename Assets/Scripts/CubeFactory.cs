using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CubeFactory : MonoBehaviour
{
    [SerializeField]
    private Cube cubePrefab;

    [SerializeField]
    private List<Color> indexToColors;

    private void Start()
    {
        for(var i = 0; i < indexToColors.Count; i++)
        {
            var logMessage = string.Format("Color {0}, is {1}", i, indexToColors[i]);
            Debug.Log(logMessage);
        }
    }

    public Cube GetCube(IDictionary<CubeSide, int> cubeSideToColorIndex)
    {
        var cubeInstance = Instantiate(cubePrefab);
        var sideToColor = new Dictionary<CubeSide, Color>();
        foreach(var side in cubeSideToColorIndex)
        {
            sideToColor.Add(side.Key, indexToColors[side.Value]);
        }

        cubeInstance.SetColors(sideToColor);
        cubeInstance.CubeSideToColorIndex = sideToColor;
        return cubeInstance;
    }
}
