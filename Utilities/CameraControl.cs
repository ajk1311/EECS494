using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CameraControl : MonoBehaviour {

	public float ScrollWidth = 15;
	public float ScrollSpeed = 25;
	public float MaxCameraHeight = 40;
	public float MinCameraHeight = 5;

	private Vector3 movement, cameraDirection;

    // Update is called once per frame
    void LateUpdate() {
            MoveCamera();
    }
    
    private void MoveCamera() {
		UpdateDirection();

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
        
		if(Input.GetAxis("Mouse ScrollWheel") <= 0 && Camera.main.transform.position.y <= MaxCameraHeight){
			movement = Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
		}
		if(Input.GetAxis("Mouse ScrollWheel") > 0 && Camera.main.transform.position.y >= MinCameraHeight){
			movement = Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
		}

		/*
        //Make sure movement is in the direction the camera is pointing
        //but ignore the vertical tilt of the camera to get sensible scrolling
        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0;
        
        //Away from ground movement
		float distance = ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");
        
        //Calculate desired camera position based on received input
        Vector3 origin = Camera.main.transform.position;
        Vector3 destination = origin;
        destination.x += movement.x;
        destination.y += movement.y;
        destination.z += movement.z;
        
        //Limit away from ground movement to be between a minimum and maximum distance
        if (destination.y > MaxCameraHeight) {
            destination.y = MaxCameraHeight;
        } else if (destination.y < MinCameraHeight) {
            destination.y = MinCameraHeight;
        }
        
        //If a change in position is detected perform the necessary update
        if (destination != origin) {
            Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ScrollSpeed);
        }*/
	}

	void FixedUpdate(){
		rigidbody.velocity = movement;
	}

	private void UpdateDirection(){
		Ray ray = Camera.main.ScreenPointToRay(Camera.main.transform.position);
		cameraDirection = ray.direction.normalized;
	}
}