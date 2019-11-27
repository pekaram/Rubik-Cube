using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CubeFace : MonoBehaviour
{
    public event Action<CubeFace> OnCubeSidePressed;

    public event Action<CubeFace> OnSideReleased;

    public CubeSide CubeSide { get; set; }

    public void OnMouseDown()
    {
        this.OnCubeSidePressed?.Invoke(this);
    }

    public void OnMouseUp()
    {
        this.OnSideReleased?.Invoke(this);
    }
}
