using UnityEngine;
using System.Collections;

public class PlayerAnimator : MonoBehaviour {

    private Animator anim;


    // Use this for initialization
    void Start () {
        anim = GetComponent<Animator>();

	}
	
	// Update is called once per frame
	void FixedUpdate () {

        Debug.Log(Input.GetAxis("Horizontal") + " "+ Input.GetAxis("Vertical"));

        if (Input.GetButton("Run"))
        {
            anim.SetFloat("VelocityX", Input.GetAxis("Horizontal")*2);
            anim.SetFloat("VelocityY", Input.GetAxis("Vertical")*2);
        } else
        {   
            anim.SetFloat("VelocityX", Input.GetAxis("Horizontal"));
            anim.SetFloat("VelocityY", Input.GetAxis("Vertical"));
        } 

	}
}
