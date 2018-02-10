using UnityEngine;
using System.Collections;

/// <summary>
/// Control camera rotations, zooming, and snapping the player to face camera direction on movement
/// </summary>
public class CameraController : MonoBehaviour {

    public Transform playerCamera, player, centerPoint;

    private float mouseX, mouseY;
    public float mouseSensitivity = 22f;
    public float mouseYPosition = 1f;
    private float zoom;
    public float zoomSpeed = 2;

    public float zoomMin = -2f;
    public float zoomMax = -10;

    public float rotationSpeed = 5f;
    
	void Start () {
        zoom = -7;
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

        //camera looks at where mouse is pointing at any give frame
        mouseX += Input.GetAxis("Mouse X");
        mouseY += Input.GetAxis("Mouse Y");

        //math clamp stop camera from going all the way around
        mouseY = Mathf.Clamp(mouseY, -60f, 60f);
        playerCamera.LookAt(centerPoint);
        centerPoint.localRotation = Quaternion.Euler(-mouseY, mouseX, 0);
        centerPoint.position = new Vector3(player.position.x, player.position.y + mouseYPosition, player.position.z);


        //rotate the player to face the camera direction
        if(Input.GetAxis("Vertical") > 0 || Input.GetAxis("Horizontal") < 0)
        {
            Quaternion turnAngle = Quaternion.Euler(0, centerPoint.eulerAngles.y, 0);

            player.rotation = Quaternion.Slerp(player.rotation, turnAngle, Time.deltaTime * rotationSpeed);
        }

	}
}
