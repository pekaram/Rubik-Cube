using System.Collections.Generic;
using UnityEngine;

public class CubesLayer : MonoBehaviour
{
    public RubikCube ParentCube { get; set; }

    public List<Cube> LayerCubes = new List<Cube>();    

    public List<Cube> GetVerticalEdges()
    {
        var cubes = new List<Cube>();

        for (int cubeIndex = 0; cubeIndex < LayerCubes.Count; cubeIndex += ParentCube.Size)
        {
            cubes.Add(this.LayerCubes[cubeIndex]);
        }

        for (int cubeIndex = ParentCube.Size - 1; cubeIndex < LayerCubes.Count; cubeIndex += ParentCube.Size)
        {
            cubes.Add(this.LayerCubes[cubeIndex]);
        }

        return cubes;
    }
}
