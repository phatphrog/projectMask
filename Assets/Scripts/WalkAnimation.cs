using UnityEngine;
using System.Collections;

public class WalkAnimation : MonoBehaviour {

    private Animator anim;
    private float vertInput;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        vertInput = Input.GetAxis("Vertical");
        anim.SetFloat("walk", vertInput); //set transition to start playing walk animation
	}
}
