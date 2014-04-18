using UnityEngine;
using System.Collections;
using RTS;

[RequireComponent(typeof(Rigidbody))]
public class CameraControl : MonoBehaviour {

    public float ScrollWidth = 15;
    public float ScrollSpeed = 25;
	public float MapClamp = 2;
    public float PanningSpeed = 200;
    public float MaxCameraHeight = 20;
    public float MinCameraHeight = 5;
    private Vector3 movement, cameraDirection;
    public float MapHeight;
    public float MapWidth;
    public Vector3 LeftTopCorner;
	public Vector3 RightTopCorner;
    public Vector3 RightBottomCorner;
    public GUIManager guiManager;
    private bool Panning;
	private bool goingToBase;
	public float speed;

	private Vector3 startPosition;

	public Vector3 StartPosition {
		get {
			return startPosition;
		}
		set {
			startPosition = value;
			transform.position = startPosition;
		}
	}

    void Start() {
		speed = 6;
        MapHeight = RTSGameMechanics.GetMapSizes().z;
        MapWidth = RTSGameMechanics.GetMapSizes().x;
    }

    void Update() {
        MoveCamera();
        if (!Panning) {
			guiManager.SetCursorState(CursorState.Select);
		}
		if (Input.GetKeyDown(KeyCode.B) && !goingToBase) {
			goingToBase = true;
		}
		if (goingToBase) {
			transform.position = Vector3.Lerp(transform.position, startPosition, speed * Time.deltaTime);
			if (Vector3.Distance(transform.position, startPosition) < 0.2f) {
				goingToBase = false;
			}
		}
    }
    
    private void MoveCamera() {
        if (GUIManager.Dragging) {
            return;
        }

        float horizontal = Input.mousePosition.x;
        float vertical = Input.mousePosition.y;
        LeftTopCorner = RTSGameMechanics.FindHitPointOnMap(new Vector3(0, Screen.height, 0));
		RightTopCorner = RTSGameMechanics.FindHitPointOnMap (new Vector3 (Screen.width, Screen.height, 0));
        RightBottomCorner = RTSGameMechanics.FindHitPointOnMap(new Vector3(Screen.width, 0, 0));
        Panning = false;
        movement = new Vector3(0, 0, 0);

        //Horizontal camera movement
        if (horizontal >= 0 && horizontal < ScrollWidth) {
            guiManager.SetCursorState(CursorState.PanLeft);
            Panning = true;
            if (LeftTopCorner.x > MapClamp && LeftTopCorner != MechanicResources.InvalidPosition)
                movement.x -= 1f;
        } else if (horizontal <= Screen.width && horizontal > Screen.width - ScrollWidth) {
            guiManager.SetCursorState(CursorState.PanRight);
            Panning = true;
            if (RightTopCorner.x < MapWidth - MapClamp && RightTopCorner != MechanicResources.InvalidPosition)
                movement.x += 1f;
        }
        
        //Vertical camera movement
        if (vertical >= 0 && vertical < ScrollWidth) {
            guiManager.SetCursorState(CursorState.PanDown);
            Panning = true;
            if (RightBottomCorner.z > MapClamp && RightBottomCorner != MechanicResources.InvalidPosition)
                movement.z -= 1f;
        } else if (vertical <= Screen.height && vertical > Screen.height - ScrollWidth) {
            guiManager.SetCursorState(CursorState.PanUp);
            Panning = true;
            if (LeftTopCorner.z < MapHeight - MapClamp && LeftTopCorner != MechanicResources.InvalidPosition)
                movement.z += 1f;
        }
        
        //Zoom in and Zoom out with Scroll
		if (GUIResources.MouseInPlayingArea()) {
	        if (Input.GetAxis("Mouse ScrollWheel") < 0 && Camera.main.transform.position.y <= MaxCameraHeight) {
	            movement = Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
	        }
	        if (Input.GetAxis("Mouse ScrollWheel") > 0 && Camera.main.transform.position.y >= MinCameraHeight) {
	            movement = Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
	        }
		}
    }

    void FixedUpdate() {
        rigidbody.velocity = movement * PanningSpeed * transform.position.y / MaxCameraHeight;
    }
}