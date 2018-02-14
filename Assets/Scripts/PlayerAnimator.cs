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
    private float playerHeight;


    //initialization
    void Awake () {
        //get the animator component
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        
	 }
	
	//called once per frame
	void FixedUpdate () {

        // Debug.Log(Input.GetAxis("Horizontal") + " "+ Input.GetAxis("Vertical"));

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        playerHeight = controller.transform.position.y;

        //set player movement
        if (Input.GetButton("Run") && verticalInput >= 0)
        {
            anim.SetFloat("VelocityX", horizontalInput * 2);
            anim.SetFloat("VelocityY", verticalInput * 2);
            
        } else
        {
            anim.SetFloat("VelocityX", horizontalInput);
            anim.SetFloat("VelocityY", verticalInput);
        }

        //set player jumping
        anim.SetFloat("VelocityZ", playerHeight);


	}
}
