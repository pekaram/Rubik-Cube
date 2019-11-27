using System.Collections;
using System.Collections.Generic;
using UnityEngine;  

public enum InteractionType
{
    VerticalRotation,
    HorizontalRotation,
    LayerRotation,
    CameraRotation,
}

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField]
    private RubikCube rubikCube;
    
    private Vector2? StartPos;

    private float rotationThreshold = 0.2f;

    [SerializeField]
    private Camera mainCamera;

    private InteractionType? activeInteraction;

    private void Update()
    {
        this.HandleMouse();
    }

    private void HandleMousePress()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if (rubikCube.SelectedIndex == null)
            {
                this.activeInteraction = InteractionType.CameraRotation;
                this.mainCamera.transform.LookAt(this.rubikCube.transform);
                this.mainCamera.transform.RotateAround(this.rubikCube.CubeCenter, Vector3.up, 40 * Input.GetAxis("Mouse X"));
                return;
            }

            if (activeInteraction == null)
            {
                var verticalCubeRotation = Mathf.Abs(Input.GetAxis("Mouse Y")) > rotationThreshold;

                if (verticalCubeRotation && Input.GetKey(KeyCode.LeftShift))
                {
                    this.activeInteraction = InteractionType.LayerRotation;
                }
                else if (verticalCubeRotation)
                {
                    this.activeInteraction = InteractionType.VerticalRotation;
                }

                else if (Mathf.Abs(Input.GetAxis("Mouse X")) > rotationThreshold)
                {
                    this.activeInteraction = InteractionType.HorizontalRotation; ;
                }
            }

            if (activeInteraction == InteractionType.VerticalRotation)
            {
                var rowNumber = (int)Mathf.Floor(this.rubikCube.SelectedIndex.Value.y / this.rubikCube.Size);
                var index = (int)this.rubikCube.SelectedIndex.Value.y - (rowNumber * this.rubikCube.Size);
                var inverted = this.rubikCube.SelectedIndex.Value.x != 0;
                this.rubikCube.VisualizeRotateVertically(index, Input.GetAxis("Mouse Y") * (inverted ? 20 : -20));
            }
            else if (activeInteraction == InteractionType.HorizontalRotation)
            {
                var index = (int)Mathf.Floor(this.rubikCube.SelectedIndex.Value.y / this.rubikCube.Size) * this.rubikCube.Size;
                this.rubikCube.VisualizeRotateHorizontally(index, Input.GetAxis("Mouse X") * -20);
            }
            else if (activeInteraction == InteractionType.LayerRotation)
            {
                this.rubikCube.VisualizeLayerRotation((int)this.rubikCube.SelectedIndex.Value.x, Input.GetAxis("Mouse Y") * -20);
            }
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
