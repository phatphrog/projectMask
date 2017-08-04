using UnityEngine;
using System.Collections;

public class WalkAnimation : MonoBehaviour {

    private Animator anim;
    private float vertInput;
    private float isRunning;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        isRunning = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        vertInput = Input.GetAxis("Vertical");
        anim.SetFloat("walk", vertInput); //set transition to start playing walk animation
        if(Input.GetButton("Run"))
        {
            isRunning = 1; 
        } else
        {
            isRunning = 0;
        }
        Debug.Log(vertInput * isRunning);
        anim.SetFloat("run", vertInput*isRunning);

        if (vertInput > 0 && Input.GetButton("Run"))
        {
            
        }
	}
}
