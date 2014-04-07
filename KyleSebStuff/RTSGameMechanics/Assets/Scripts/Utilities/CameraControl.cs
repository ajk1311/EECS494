using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CameraControl : MonoBehaviour {

    public float ScrollWidth = 15;
    public float ScrollSpeed = 25;
	public float PanningSpeed = 25;
    public float MaxCameraHeight = 20;
    public float MinCameraHeight = 5;
    private Vector3 movement, cameraDirection;

    void Update() {
        MoveCamera();
    }
    
    private void MoveCamera() {
		if (GUIManager.Dragging) {
			return;
		}

        float xpos = Input.mousePosition.x;
        float ypos = Input.mousePosition.y;
        movement = new Vector3(0, 0, 0);
        
        //Horizontal camera movement
        if (xpos >= 0 && xpos < ScrollWidth) {
            movement.x -= 1.5f;
        } else if (xpos <= Screen.width && xpos > Screen.width - ScrollWidth) {
            movement.x += 1.5f;
        }
        
        //Vertical camera movement
        if (ypos >= 0 && ypos < ScrollWidth) {
            movement.z -= 1.5f;
        } else if (ypos <= Screen.height && ypos > Screen.height - ScrollWidth) {
            movement.z += 1.5f;
        }
        
		//Zoom in and Zoom out with Scroll
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && Camera.main.transform.position.y <= MaxCameraHeight) {
            movement = Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && Camera.main.transform.position.y >= MinCameraHeight) {
            movement = Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
        }
    }

    void FixedUpdate() {
        rigidbody.velocity = movement * PanningSpeed;
    }
}