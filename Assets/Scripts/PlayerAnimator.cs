using UnityEngine;
using System.Collections;

/// <summary>
/// Controls player animation input vars
/// </summary>
public class PlayerAnimator : MonoBehaviour {

    private Animator anim;
    private float verticalInput;
    private float horizontalInput;


    //initialization
    void Start () {
        //get the animator component
        anim = GetComponent<Animator>();

	 }
	
	//called once per frame
	void FixedUpdate () {

        // Debug.Log(Input.GetAxis("Horizontal") + " "+ Input.GetAxis("Vertical"));

        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetButton("Run") && verticalInput >= 0)
        {
            anim.SetFloat("VelocityX", horizontalInput * 2);
            anim.SetFloat("VelocityY", verticalInput * 2);
        } else
        {   
            if(verticalInput < 0)
            {
                //verticalInput = verticalInput * 5;
            }
            anim.SetFloat("VelocityX", horizontalInput);
            anim.SetFloat("VelocityY", verticalInput);
        }

        //standing jump
        if (Input.GetButton("Jump") && verticalInput == 0 && horizontalInput == 0)
        {
            anim.SetFloat("VelocityZ", 1);
        }
        else
        {
            anim.SetFloat("VelocityZ", 0);
        }

	}
}
