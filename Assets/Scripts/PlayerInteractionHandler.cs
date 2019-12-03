using System.Collections;
using System.Collections.Generic;
using UnityEngine;  

public enum InteractionType
{
    VerticalRotation,
    HorizontalRotation,
    LayerRotation,
    HorizontalCameraRotation,
    VerticalCameraRotation
}

public enum PullDirection
{
    Horizontal,
    Vertical,
    None,
}

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField]
    private RubikCube rubikCube;
    
    private float rotationThreshold = 0.2f;

    [SerializeField]
    private Camera mainCamera;

    private InteractionType? activeInteraction;

    private Dictionary<Vector2, CubeSide> layerRotationHandles = new Dictionary<Vector2, CubeSide>();

    private PullDirection pullDirection;

    private void Update()
    {
        this.HandlePullDirection();

        this.HandleMouse();
    }

    private void HandleMousePress()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {    
            if (activeInteraction == null)
            {
                if (rubikCube.SelectedIndex == null)
                {
                    switch(pullDirection)
                    {
                        case PullDirection.Horizontal:
                            activeInteraction = InteractionType.HorizontalCameraRotation;
                            return;

                        case PullDirection.Vertical:
                            activeInteraction = InteractionType.VerticalCameraRotation;
                            return;
                    }

                    return;
                }

                var isLayerHandle =
                    this.rubikCube.VerticalEdges.Contains(this.rubikCube.SelectedCube) &&
                    (this.rubikCube.selctedFace == CubeSide.Left || this.rubikCube.selctedFace == CubeSide.Right);

                if (pullDirection == PullDirection.Vertical && isLayerHandle)
                {
                    this.activeInteraction = InteractionType.LayerRotation;
                }
                else if (pullDirection == PullDirection.Vertical)
                {
                    this.activeInteraction = InteractionType.VerticalRotation;
                }
                else if (pullDirection == PullDirection.Horizontal)
                {
                    this.activeInteraction = InteractionType.HorizontalRotation;
                }
            }

            this.Handle();
        }
    }

    private void HandleVerticalRotation()
    {
        var rowNumber = (int)Mathf.Floor(this.rubikCube.SelectedIndex.Value.y / this.rubikCube.Size);
        var index = (int)this.rubikCube.SelectedIndex.Value.y - (rowNumber * this.rubikCube.Size);
        var inverted = this.rubikCube.selctedFace == CubeSide.Back ? true : false;
        this.rubikCube.VisualizeRotateVertically(index, Input.GetAxis("Mouse Y") * (inverted ? 20 : -20));
    }

    private void HandleHorizontalRotation()
    {
        var index = (int)Mathf.Floor(this.rubikCube.SelectedIndex.Value.y / this.rubikCube.Size) * this.rubikCube.Size;
        this.rubikCube.VisualizeRotateHorizontally(index, Input.GetAxis("Mouse X") * -20);
    }

    private void HandleLayerRotation()
    {
        var inverted = this.rubikCube.selctedFace == CubeSide.Right ? true : false;
        this.rubikCube.VisualizeLayerRotation((int)this.rubikCube.SelectedIndex.Value.x, Input.GetAxis("Mouse Y") * (inverted ? 20 : -20));
    }

    private void Handle()
    {
        switch(activeInteraction)
        {
            case InteractionType.VerticalRotation:
                this.HandleVerticalRotation();
                return;

            case InteractionType.HorizontalRotation:
                this.HandleHorizontalRotation();
                return;

            case InteractionType.LayerRotation:
                this.HandleLayerRotation();
                return;

            case InteractionType.VerticalCameraRotation:
                this.RotateCamera();
                return;

            case InteractionType.HorizontalCameraRotation:
                this.RotateCamera();
                return;
        }
    }

    private void RotateCamera()
    {
        var vector = default(Vector3);
        var axis = string.Empty;
        if(this.activeInteraction == InteractionType.HorizontalCameraRotation)
        {
            vector = Vector3.up;
            axis = "Mouse X";
        }
        else
        {
            vector = this.mainCamera.transform.right;
            axis = "Mouse Y";
        }

        this.mainCamera.transform.RotateAround(this.rubikCube.CubeCenter, vector, 40 * Input.GetAxis(axis));
    }

    private void HandlePullDirection()
    {
        var isVerticalPull = Mathf.Abs(Input.GetAxis("Mouse Y")) > rotationThreshold;
        var isHorizontalPull = Mathf.Abs(Input.GetAxis("Mouse X")) > rotationThreshold;
        if(isVerticalPull)
        {
            this.pullDirection = PullDirection.Vertical;
        }
        else if(isHorizontalPull)
        {
            this.pullDirection = PullDirection.Horizontal;
        }
        else
        {
            this.pullDirection = PullDirection.None;
        }
    }

    private void HandleMouse()
    {
        this.HandleMousePress();
        this.HandleMouseReleased();
    }   

    private void HandleMouseReleased()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            if (this.activeInteraction == null)
            {
                this.rubikCube.ResetSelectedIndex();
                return;
            }

            switch (activeInteraction)
            {
                case InteractionType.VerticalRotation:
                    this.rubikCube.OnVerticalRotationDone();
                    break;
                case InteractionType.HorizontalRotation:
                    this.rubikCube.OnHorizontalRotationDone();
                    break;
                case InteractionType.LayerRotation:
                    this.rubikCube.OnLayerRotationEnded();
                    break;
            }

            this.rubikCube.ResetSelectedIndex();
            this.activeInteraction = null;
        }
    }
}
