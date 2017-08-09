using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    public Transform playerCamera, player, centerPoint;

    private float mouseX, mouseY;
    public float mouseSensitivity = 22f;
    public float mouseYPosition = 1f;

    private float verticalInput;
    private float horizontalInput;

    private float moveFrontBack, moveLeftRight;
    public float walkSpeed = 4f;
    private float moveSpeed;

    private float zoom;
    public float zoomSpeed = 2;

    public float zoomMin = -2f;
    public float zoomMax = -10;

    public float rotationSpeed = 5f;
    
	void Start () {
        zoom = -7;
        moveSpeed = walkSpeed;
	}

    // Update is called once per frame
    void FixedUpdate() {

        zoom += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        if (zoom > zoomMin)
        {
            zoom = zoomMin;
        }

        if (zoom < zoomMax)
        {
            zoom = zoomMax;
        }

        //control camera's zoom in and out from the player
        playerCamera.transform.localPosition = new Vector3(0, 0, zoom);

        if (Input.GetMouseButton(1))
        {
            mouseX += Input.GetAxis("Mouse X");
            mouseY += Input.GetAxis("Mouse Y");
        }

        //math clamp stop camera from going all the way around
        mouseY = Mathf.Clamp(mouseY, -60f, 60f);
        playerCamera.LookAt(centerPoint);
        centerPoint.localRotation = Quaternion.Euler(-mouseY, mouseX, 0);

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        if (verticalInput > 0 && Input.GetButton("Run") || horizontalInput != 0 && Input.GetButton("Run") && verticalInput >= 0)
        {
            //running
            moveSpeed = walkSpeed * 3;

        } else if(verticalInput < 0)
        {
            //walking backwards
            moveSpeed = walkSpeed * 0.6f;
        } else
        {
            //walking
            moveSpeed = walkSpeed;
        }

        moveFrontBack = verticalInput * moveSpeed;
        moveLeftRight = horizontalInput * moveSpeed;

        Vector3 movement = new Vector3(moveLeftRight, 0, moveFrontBack);
        movement = player.rotation * movement; 
        player.GetComponent<CharacterController>().Move(movement * Time.deltaTime);
        centerPoint.position = new Vector3(player.position.x, player.position.y + mouseYPosition, player.position.z);

        if(Input.GetAxis("Vertical") > 0 || Input.GetAxis("Horizontal") < 0)
        {
            Quaternion turnAngle = Quaternion.Euler(0, centerPoint.eulerAngles.y, 0);

            player.rotation = Quaternion.Slerp(player.rotation, turnAngle, Time.deltaTime * rotationSpeed);
        }

	}
}
