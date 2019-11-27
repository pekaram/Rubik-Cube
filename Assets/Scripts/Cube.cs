using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public struct CubeSideObject
{
    public Renderer renderer;
    public CubeSide Side;
}

public enum CubeSide
{
    Left,
    Right,
    Top,
    Bottom,
    Back,
    Front
}

public class Cube : MonoBehaviour
{
    [SerializeField]
    private List<CubeSideObject> cubeSides;

    public event Action<Cube, CubeFace> OnCubeClicked;

    public event Action<Cube, CubeFace> OnCubeReleased;

    public IDictionary<CubeSide, Color> CubeSideToColorIndex = new Dictionary<CubeSide, Color>();
    
    private IDictionary<CubeSide, CubeSide> verticalSwitches = new Dictionary<CubeSide, CubeSide>
        {
            {CubeSide.Bottom, CubeSide.Front },
            {CubeSide.Front, CubeSide.Top },
            {CubeSide.Top, CubeSide.Back },
            {CubeSide.Back, CubeSide.Bottom }
        };

    private IDictionary<CubeSide, CubeSide>  horizontalSwitches = new Dictionary<CubeSide, CubeSide>
        {
            {CubeSide.Right, CubeSide.Front },
            {CubeSide.Back, CubeSide.Right },
            {CubeSide.Front, CubeSide.Left },
            {CubeSide.Left, CubeSide.Back },
        };

    private IDictionary<CubeSide, CubeSide> layerSwitches = new Dictionary<CubeSide, CubeSide>()
        {
            {CubeSide.Top, CubeSide.Left },
            {CubeSide.Left, CubeSide.Bottom },
            {CubeSide.Bottom, CubeSide.Right },
            {CubeSide.Right, CubeSide.Top }
        };

    private void Awake()
    {
        foreach(var sideObject in cubeSides)
        {
            var faceData = sideObject.renderer.gameObject.GetComponent<CubeFace>();
            faceData.CubeSide = sideObject.Side;
            faceData.OnCubeSidePressed += OnCubesSidePressed;
            faceData.OnSideReleased += OnFaceReleased;
        }
    }

    private void OnFaceReleased(CubeFace side)
    {
        this.OnCubeReleased?.Invoke(this, side);
    }

    private void OnCubesSidePressed(CubeFace side)
    {
        this.OnCubeClicked?.Invoke(this, side);
    }

    public void SetColors(IDictionary<CubeSide, Color> sideToColor)
    {
        foreach (var side in cubeSides)
        {
            if(!sideToColor.ContainsKey(side.Side))
            {
                side.renderer.material.SetColor("_Color", Color.black);

                continue;
            }

            side.renderer.material.SetColor("_Color", sideToColor[side.Side]);
        }

        this.CubeSideToColorIndex = sideToColor;
    }
    
    public Color GetFaceColor(CubeSide sideToLookFrom)
    {
        return this.CubeSideToColorIndex[sideToLookFrom];
    }

    public void RotateVertically(bool switchColors = false)
    {
        this.Switch(this.verticalSwitches, switchColors);
    }

    public void RotateHorizontally(bool switchColors = false)
    {    
        this.Switch(this.horizontalSwitches, switchColors);
    }

    public void RotateInLayer(bool switchColors = false)
    {
        this.Switch(this.layerSwitches, switchColors);
    }

    private void Switch(IDictionary<CubeSide, CubeSide> switches, bool switchColors = false)
    {
        var tempDictionary = new Dictionary<CubeSide, Color>(this.CubeSideToColorIndex);

        foreach (var side in switches)
        {
            if (!this.CubeSideToColorIndex.ContainsKey(side.Value))
            {
                tempDictionary.Remove(side.Key);
                continue;
            }

            tempDictionary[side.Key] = this.CubeSideToColorIndex[side.Value];
        }

        this.CubeSideToColorIndex = tempDictionary;
        if (switchColors)
        {
            this.SetColors(this.CubeSideToColorIndex);
        }
    }
}
