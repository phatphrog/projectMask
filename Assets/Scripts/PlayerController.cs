using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Transform playerCamera, player, centerPoint;

    private float mouseX, mouseY;
    public float mouseSensitivity = 22f;
    public float mouseYPosition = 1f;

    private float moveFrontBack, moveLeftRight;
    public float moveSpeed = 4f;

    private float zoom;
    public float zoomSpeed = 2;

    public float zoomMin = -2f;
    public float zoomMax = -10;

    public float rotationSpeed = 5f;

    private bool isRunning = false;
    
	void Start () {
        zoom = -7;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

        zoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        if(zoom > zoomMin)
        {
            zoom = zoomMin;
        }
	    
        if(zoom < zoomMax)
        {
            zoom = zoomMax;
        }

        //control camera's zoom in and out from the player
        playerCamera.transform.localPosition = new Vector3(0, 0, zoom);

        if(Input.GetMouseButton(1))
        {
            mouseX += Input.GetAxis("Mouse X");
            mouseY += Input.GetAxis("Mouse Y");
        }

        //math clamp stop camera from going all the way around
        mouseY = Mathf.Clamp(mouseY, -60f, 60f);
        playerCamera.LookAt(centerPoint);
        centerPoint.localRotation = Quaternion.Euler(-mouseY, mouseX, 0);

        if(Input.GetAxis("Vertical") > 0 && Input.GetButton("Run"))
        {
            isRunning = true; 
        } else
        {
            isRunning = false;
        }

        if(isRunning)
        {
            moveFrontBack = Input.GetAxis("Vertical") * moveSpeed*3;
            moveLeftRight = Input.GetAxis("Horizontal") * moveSpeed*3;
        } else
        {
            moveFrontBack = Input.GetAxis("Vertical") * moveSpeed;
            moveLeftRight = Input.GetAxis("Horizontal") * moveSpeed;
        }
         
        Vector3 movement = new Vector3(moveLeftRight, 0, moveFrontBack);
        movement = player.rotation * movement; 
        player.GetComponent<CharacterController>().Move(movement * Time.deltaTime);
        centerPoint.position = new Vector3(player.position.x, player.position.y + mouseYPosition, player.position.z);

        if(Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") < 0)
        {
            Quaternion turnAngle = Quaternion.Euler(0, centerPoint.eulerAngles.y, 0);

            player.rotation = Quaternion.Slerp(player.rotation, turnAngle, Time.deltaTime * rotationSpeed);
        }

	}
}
