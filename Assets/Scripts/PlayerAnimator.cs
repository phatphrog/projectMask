using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour {

    private Animator anim;
    private float verticalInput;
    private float horizontalInput;


    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();

	}
	
	// Update is called once per frame
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

	}
}
