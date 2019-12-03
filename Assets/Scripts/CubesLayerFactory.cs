using System.Collections.Generic;
using UnityEngine;

public enum LayerType
{
    Front,
    Middle,  
    Back,  
}

public class CubesLayerFactory : MonoBehaviour
{
    [SerializeField]
    private CubesLayer layerPrefab;

    [SerializeField]
    private CubeFactory cubeFactory;

    private const float MarginBetweenCubes = 0.2f;
   
    public CubesLayer CreateLayer(int width, LayerType layerType, float margin, Dictionary<CubeSide, int> sideToColorIndex, RubikCube parentCube)
    {
        var layer = Instantiate(layerPrefab);

        for (var i = 0; i < width * width; i++)
        {
            var rowNumber = Mathf.Floor(i / width);
            var isEdgeRow = rowNumber % (width - 1) == 0;
            var isFirstRow = isEdgeRow && rowNumber == 0;
            var isLastRow = isEdgeRow && !isFirstRow;

            var isFirstInRow = i % (width) == 0;
            var isLastInRow = (i + 1) - (width * (rowNumber + 1)) == 0;
            
            var cubeSides = new List<CubeSide>();
           
            if(isFirstInRow)
            {
                cubeSides.Add(CubeSide.Left);
            }
            else if(isLastInRow)
            {
                cubeSides.Add(CubeSide.Right);
            }

            if(isFirstRow)
            {
                cubeSides.Add(CubeSide.Top);
            }
            else if(isLastRow)
            {
                cubeSides.Add(CubeSide.Bottom);
            }

            if (layerType == LayerType.Front)
            {
                cubeSides.Add(CubeSide.Front);
            }
            else if (layerType == LayerType.Back)
            {
                cubeSides.Add(CubeSide.Back);
            }

            var coloredSides = new Dictionary<CubeSide, int>();
            foreach (var side in cubeSides)
            {
                coloredSides.Add(side, sideToColorIndex[side]);
            }
            
            var cube = cubeFactory.GetCube(coloredSides);

            cube.transform.SetParent(layer.transform);
            cube.transform.Translate((i - (width* rowNumber))  * (1 + MarginBetweenCubes), -rowNumber - (MarginBetweenCubes * rowNumber), margin);
            layer.LayerCubes.Add(cube);
        }

        layer.ParentCube = parentCube;
        return layer;
    }

    public CubesLayer GetBlankLayer()
    {
        return Instantiate(layerPrefab);
    }

}
