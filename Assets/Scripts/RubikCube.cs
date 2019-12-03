using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RubikCube : MonoBehaviour
{
    [SerializeField]
    private CubesLayerFactory cubesLayerFactory;

    [SerializeField, Range(2, 6)]
    private int cubeSize;

    [SerializeField]
    private GameObject winScreen;

    public int Size
    {
        get { return this.cubeSize; }
    }

    private IDictionary<int, CubesLayer> indexToLayer = new Dictionary<int, CubesLayer>();

    private IDictionary<int, IDictionary<int, Vector3>> cubeIndexToPosition = new Dictionary<int, IDictionary<int, Vector3>>();

    public Vector2? SelectedIndex { get; set; }

    public CubeSide? selctedFace { get; set; }

    private CubesLayer rotationLayer;

    private Quaternion originalRotation;

    private Vector3 pos;

    public Vector3 CubeCenter { get; set; }

    Dictionary<Cube, CubesLayer> adoptedCubesToOriginalParent = new Dictionary<Cube, CubesLayer>();

    private void Start()
    {
        this.CreateCube();

        this.RegisterChildCubes();

        this.InitializeRotationLayer();
        this.Randomize();
    }

    private void Randomize()
    {
        var horizontalIndex = 0;

        for (var i = 0; i < 20; i++)
        {
            var numberOfVerticalRounds = Random.Range(0, 4);
            var horizontalNumberofRounds = Random.Range(0, 4);

            var verticalIndex = Random.Range(0, this.indexToLayer.Count -1);


            this.RotateVerticallyClockWise(verticalIndex, true);
            this.RotateHorizontally(horizontalIndex, true);
            if(horizontalIndex + this.indexToLayer.Count <= (this.indexToLayer.Count * this.indexToLayer.Count - 2))
            {
                horizontalIndex += this.indexToLayer.Count;
            }
            else
            {
                horizontalIndex = 0;
            }            
        }
    }

    private void RegisterChildCubes()
    {
        for (var i = 0; i < indexToLayer.Count; i++)
        {
            this.cubeIndexToPosition[i] = new Dictionary<int, Vector3>();

            for (var j = 0; j < indexToLayer[i].LayerCubes.Count; j++)
            {
                this.cubeIndexToPosition[i][j] = indexToLayer[i].LayerCubes[j].transform.position;

                this.indexToLayer[i].LayerCubes[j].OnCubeClicked += this.OnCubeClicked;
            }
        }
    }

    private void InitializeRotationLayer()
    {
        this.rotationLayer = this.cubesLayerFactory.GetBlankLayer();
        originalRotation = this.rotationLayer.transform.rotation;
        pos = this.rotationLayer.transform.position;
        this.rotationLayer.name = "Temp Rotation Layer";

        this.CubeCenter = (this.cubeIndexToPosition
            [this.cubeIndexToPosition.Count - 1]
            [(this.cubeIndexToPosition.Count * this.cubeIndexToPosition.Count) - 1] - 
            this.cubeIndexToPosition[0][0]) / 2;
    }
    
    private void CreateCube()
    {
        for (var i = 0; i < cubeSize; i++)
        {
            var layerType = default(LayerType);
            if (i == 0)
            {
                layerType = LayerType.Front;
            }
            else if (i == cubeSize - 1)
            {
                layerType = LayerType.Back;
            }
            else
            {
                layerType = LayerType.Middle;
            }

            var layer = this.cubesLayerFactory.CreateLayer(
            cubeSize,
            layerType,
            i * 1.2f,
            new Dictionary<CubeSide, int>
            {
                { CubeSide.Back, 0 },
                { CubeSide.Front, 1 },
                { CubeSide.Left, 2 },
                { CubeSide.Right, 3 },
                { CubeSide.Top, 5},
                { CubeSide.Bottom, 6 }
            });

            layer.name = "layer" + i;
            layer.transform.SetParent(this.transform);
            this.indexToLayer.Add(i, layer);
        }

    }

    private void OnLayerRotationStarted(int layerIndex)
    {
        this.adoptedCubesToOriginalParent = new Dictionary<Cube, CubesLayer>();
        foreach (var cube in this.indexToLayer[layerIndex].LayerCubes)
        {
            cube.transform.SetParent(this.rotationLayer.transform);

            this.adoptedCubesToOriginalParent.Add(cube, this.indexToLayer[layerIndex]);
        }
    }

    public void VisualizeLayerRotation(int layerIndex, float degree)
    {
        if (this.adoptedCubesToOriginalParent.Count == 0)
        {
            this.OnLayerRotationStarted(layerIndex);
        }

        this.rotationLayer.transform.RotateAround(
            (this.cubeIndexToPosition[layerIndex][(this.indexToLayer.Count * this.indexToLayer.Count) - 1]
            - this.cubeIndexToPosition[layerIndex][0]) / 2, 
            Vector3.forward, 
            degree);
    }

    public void OnLayerRotationEnded()
    {
        var turns = Mathf.RoundToInt(this.rotationLayer.transform.eulerAngles.z / 90);

        this.ResetRotationLayer();
        
        for (var i=0; i < 4 - turns;i++)
        {
            this.RotateLayer((int)this.SelectedIndex.Value.x, true);
        }

        this.OnRotationDone();
    }

    private void OnVerticalRotationStarted(int CubeIndex)
    {
        this.adoptedCubesToOriginalParent = new Dictionary<Cube, CubesLayer>();
        for (var i = 0; i < this.indexToLayer.Count; i++)
        {
            for (var j = CubeIndex; j < this.indexToLayer[i].LayerCubes.Count + 1 + CubeIndex - this.indexToLayer.Count; j += this.indexToLayer.Count)
            {
                var cube = this.indexToLayer[i].LayerCubes[j];
                cube.transform.SetParent(this.rotationLayer.transform);

                this.adoptedCubesToOriginalParent.Add(cube, this.indexToLayer[i]);
            }
        }
    }

    private void OnHorizontalRotationStarted(int CubeIndex)
    {
        this.adoptedCubesToOriginalParent = new Dictionary<Cube, CubesLayer>();
        for (var i = 0; i < this.indexToLayer.Count; i++)
        {
            for (var j = CubeIndex; j < CubeIndex + this.indexToLayer.Count; j ++)
            {
                var cube = this.indexToLayer[i].LayerCubes[j];
                cube.transform.SetParent(this.rotationLayer.transform);

                this.adoptedCubesToOriginalParent.Add(cube, this.indexToLayer[i]);
            }
        }
    }

    public void VisualizeRotateHorizontally(int CubeIndex, float degree)
    {
        if (this.adoptedCubesToOriginalParent.Count == 0)
        {
            this.OnHorizontalRotationStarted(CubeIndex);
        }

        var midPoint = (this.cubeIndexToPosition[this.indexToLayer.Count-1][CubeIndex + indexToLayer.Count -1] - this.cubeIndexToPosition[0][CubeIndex]) / 2;
        this.rotationLayer.transform.RotateAround(midPoint, Vector3.up, degree);
    }

    public void VisualizeRotateVertically(int CubeIndex, float degree)
    {
        if (this.adoptedCubesToOriginalParent.Count == 0)
        {
            this.OnVerticalRotationStarted(CubeIndex);
        }
        
        var midPoint = (this.cubeIndexToPosition[this.indexToLayer.Count - 1][this.indexToLayer.Count * this.indexToLayer.Count - this.indexToLayer.Count + CubeIndex] - this.cubeIndexToPosition[0][CubeIndex]) / 2;
        this.rotationLayer.transform.RotateAround(midPoint,Vector3.left, degree);      

    }

    public void OnVerticalRotationDone()
    {
        var rowNumber = (int)Mathf.Floor(this.SelectedIndex.Value.y / this.Size);
        var index = (int)this.SelectedIndex.Value.y - (rowNumber * this.Size);
        var turns = Mathf.Round(this.rotationLayer.transform.eulerAngles.x / 90);

        // TODO: improve this
        if (this.rotationLayer.transform.eulerAngles.y == 180 && turns ==4)
        {
            turns = 2;          
        }

        this.ResetRotationLayer(); 
      
        for (var i =0; i<4-turns; i++)
        {
            RotateVerticallyClockWise(index, true);
        }

        this.OnRotationDone();
    }

    private void OnRotationDone()
    {
        foreach (var cube in this.adoptedCubesToOriginalParent)
        {
            cube.Key.transform.SetParent(cube.Value.transform);
        }

        this.adoptedCubesToOriginalParent.Clear();

        if (this.IsFrontFaceSolved() &&
            this.IsBackFaceSolved() &&
            this.IsLeftFaceSolved() &&
            this.IsRightSolved())
        {
            this.winScreen.SetActive(true);
        }
    }

    private bool IsFrontFaceSolved()
    {
        var referenceCube = this.indexToLayer[0].LayerCubes[0];
        var refColor = referenceCube.CubeSideToColorIndex[CubeSide.Front];
        foreach (var cube in this.indexToLayer[0].LayerCubes)
        {
            var color = cube.CubeSideToColorIndex[CubeSide.Front];
            if (refColor != color)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsBackFaceSolved()
    {
        var lastLayerIndex = indexToLayer.Count - 1;
        var referenceCube = this.indexToLayer[lastLayerIndex].LayerCubes[0];
        var refColor = referenceCube.CubeSideToColorIndex[CubeSide.Back];
        foreach (var cube in this.indexToLayer[lastLayerIndex].LayerCubes)
        {
            var color = cube.CubeSideToColorIndex[CubeSide.Back];
            if (refColor != color)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsLeftFaceSolved()
    {
        var lastLayerIndex = indexToLayer.Count - 1;
        var referenceCube = this.indexToLayer[0].LayerCubes[0];
        var refColor = referenceCube.CubeSideToColorIndex[CubeSide.Left];

        for (var layer = 0; layer < this.indexToLayer.Count; layer++)
        {
            for (var cube = 0; cube <= (this.indexToLayer.Count * this.indexToLayer.Count) - this.indexToLayer.Count; cube += this.indexToLayer.Count)
            {
                var color = this.indexToLayer[layer].LayerCubes[cube].CubeSideToColorIndex[CubeSide.Left];
                if (refColor != color)
                {
                    return false;
                }
            }

        }
        return true;
    }

    private bool IsRightSolved()
    {
        var lastLayerIndex = indexToLayer.Count - 1;
        var referenceCube = this.indexToLayer[lastLayerIndex].LayerCubes[lastLayerIndex];
        var refColor = referenceCube.CubeSideToColorIndex[CubeSide.Right];

        for (var layer = 0; layer < this.indexToLayer.Count; layer++)
        {
            for (var cube = lastLayerIndex; cube <= (this.indexToLayer.Count * this.indexToLayer.Count); cube += this.indexToLayer.Count)
            {
                var color = this.indexToLayer[layer].LayerCubes[cube].CubeSideToColorIndex[CubeSide.Right];
                if (refColor != color)
                {
                    return false;
                }
            }
        }
        return true;
    }


    private void ResetRotationLayer()
    {
        this.rotationLayer.transform.rotation = this.originalRotation;
        this.rotationLayer.transform.position = this.pos;
    }

    public void ResetSelectedIndex()
    {
        this.SelectedIndex = null;
        this.selctedFace = null;
    }

    public void OnHorizontalRotationDone()
    {
        var index = (int)Mathf.Floor(this.SelectedIndex.Value.y / this.indexToLayer.Count) * indexToLayer.Count;
        var turns = Mathf.Round(this.rotationLayer.transform.eulerAngles.y / 90);

        this.ResetRotationLayer();

        for (var i =0; i<4-turns; i++)
        {
            RotateHorizontally(index, true);
        }

        this.OnRotationDone();
    }

    public void OnCubeClicked(Cube cube, CubeFace side)
    {
        for (var i = 0; 0 < indexToLayer.Count; i++)
        {
            for(var j =0; j<indexToLayer[i].LayerCubes.Count ;j++)
            {
                if(this.indexToLayer[i].LayerCubes[j] == cube)
                {
                    this.SelectedIndex = new Vector2(i,j);
                    this.selctedFace = side.CubeSide;
                    return;
                }
            }
        }
   
        Debug.LogError("Selected cube index wasn't found, this shouldn't happen normally");
    }

    public void RotateVerticallyClockWise(int selectedCubeIndex, bool ignoreRotateVisuals = false)
    {
        var tempLayers = new Dictionary<int, List<Cube>>();
        foreach(var layer in this.indexToLayer)
        {
            tempLayers.Add(layer.Key, this.indexToLayer[layer.Key].LayerCubes.ToList());
        }

        for (int cubeIndex = selectedCubeIndex; cubeIndex < indexToLayer.Count * indexToLayer.Count; cubeIndex += indexToLayer.Count)
        {
            for (var layerIndex = 0; layerIndex < indexToLayer.Count; layerIndex++)
            {
                var cubeToRemove = tempLayers[layerIndex][cubeIndex];
                var layerToInsertTo = cubeIndex / indexToLayer.Count;
                var cubeIndexToInsertTo = ((indexToLayer.Count * indexToLayer.Count) - indexToLayer.Count) - (layerIndex * indexToLayer.Count) + selectedCubeIndex;
                
                cubeToRemove.RotateVertically(ignoreRotateVisuals);

                this.RepositionCube(cubeToRemove, layerToInsertTo, cubeIndexToInsertTo);
            }
        }
    }

    public void RotateHorizontally(int selectedCubeIndex, bool ignoreRotateVisuals = false)
    {
        var tempLayers = new Dictionary<int, List<Cube>>();
        foreach (var layer in this.indexToLayer)
        {
            tempLayers.Add(layer.Key, this.indexToLayer[layer.Key].LayerCubes.ToList());
        }

        for (int layer = 0; layer < indexToLayer.Count; layer += 1)
        {
            for (var cubeIndex = selectedCubeIndex; cubeIndex < selectedCubeIndex + indexToLayer.Count; cubeIndex++)
            {
                var cubeToRemove = tempLayers[layer][cubeIndex];
                var layerToInsertTo = cubeIndex -selectedCubeIndex;
                var cubeIndexToInsertTo = indexToLayer.Count + selectedCubeIndex - 1 - layer;
                
                cubeToRemove.RotateHorizontally(ignoreRotateVisuals);

                this.RepositionCube(cubeToRemove, layerToInsertTo, cubeIndexToInsertTo);
            }
        }
    }

    private void RepositionCube(Cube cubeToRemove, int layerToInsertTo, int cubeIndexToInsertTo)
    {
        var location = cubeIndexToPosition[layerToInsertTo][cubeIndexToInsertTo];
        cubeToRemove.transform.SetParent(this.indexToLayer[layerToInsertTo].transform);
        cubeToRemove.transform.localPosition = location;
        cubeToRemove.name = "Cube " + layerToInsertTo + " " + cubeIndexToInsertTo;

        this.indexToLayer[layerToInsertTo].LayerCubes[cubeIndexToInsertTo] = cubeToRemove;
    }

    public void RotateLayer(int selectedLayerIndex, bool ignoreRotateVisuals = false)
    { 
        var tempLayers = new Dictionary<int, List<Cube>>();
        foreach (var layer in this.indexToLayer)
        {
            tempLayers.Add(layer.Key, this.indexToLayer[layer.Key].LayerCubes.ToList());
        }

        var width = indexToLayer[selectedLayerIndex].LayerCubes.Count/ indexToLayer.Count;
        for (var i = 0; i < width * width; i++)
        {
            var rowNumber =(int) Mathf.Floor(i / width);
            var cubeToRemove = tempLayers[selectedLayerIndex][i];  
            var layerToInsertTo = selectedLayerIndex;
            var columnIndex = indexToLayer.Count - 1 - rowNumber;
            var rowIndex = i -(rowNumber * width);
            var position = rowIndex * width + columnIndex;
            
            cubeToRemove.RotateInLayer(ignoreRotateVisuals);

            this.RepositionCube(cubeToRemove, layerToInsertTo, position);
        }    
    }
}
