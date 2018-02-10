using UnityEngine;
/// <summary>
/// PlayerController manages player movement, jumping, and gravity
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 4.0F;
    public float speed = 4.0F;
    public float jumpSpeed = 9.1F;
    public float gravity = 15.0F;
    private Vector3 moveDirection = Vector3.zero;
    CharacterController controller;
    private int count = 0;

    //Assign out controller
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }


    void  FixedUpdate()
    {

        if (controller.isGrounded)
        {


            if (Input.GetAxis("Vertical") > 0 && Input.GetButton("Run") || Input.GetAxis("Horizontal") != 0 && Input.GetButton("Run") && Input.GetAxis("Vertical") >= 0)
            {
                //running
                speed = walkSpeed * 3;

            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                //walking backwards`
                speed = walkSpeed * 0.6f;
            }
            else
            {
                //walking
                speed = walkSpeed;
            }

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;

            //TODO: make a jump cooldown

        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

}