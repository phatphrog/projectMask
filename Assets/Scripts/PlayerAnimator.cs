using UnityEngine;
using System.Collections;

/// <summary>
/// Controls player animation input vars
/// </summary>
public class PlayerAnimator : MonoBehaviour {

    private Animator anim;
    private CharacterController controller;
    private float verticalInput;
    private float horizontalInput;


    //initialization
    void Start () {
        //get the animator component
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        
	 }
	
	//called once per frame
	void FixedUpdate () {

        // Debug.Log(Input.GetAxis("Horizontal") + " "+ Input.GetAxis("Vertical"));

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetButton("Run") && verticalInput >= 0)
        {
            anim.SetBool("IsRunning", true);
            anim.SetFloat("VelocityX", horizontalInput * 2);
            anim.SetFloat("VelocityY", verticalInput * 2);
        } else
        {
            anim.SetBool("IsRunning", false);
            anim.SetFloat("VelocityX", horizontalInput);
            anim.SetFloat("VelocityY", verticalInput);
        }

        //jump
        if (Input.GetButton("Jump") && controller.isGrounded)
        {
            anim.SetFloat("VelocityZ", 1);
        }
        else
        {
            anim.SetFloat("VelocityZ", 0);
        }

	}
}
