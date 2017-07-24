using UnityEngine;
/// <summary>
/// Simple class with very simple gravity.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerPhysics : MonoBehaviour
{


    Vector3 moveVector;
    CharacterController controller;

    //Assign out controller
    void Start()
    {
        controller = GetComponent<CharacterController>();

    }


    /// <summary>
    /// Update, we set MoveVector to zero to reset it for this fram otherwise it will grow over each frame
    /// </summary>
    void Update()
    {

        //REeset the MoveVector
        moveVector = Vector3.zero;

        //Check if cjharacter is grounded
        if (controller.isGrounded == false)
        {
            //Add our gravity Vecotr
            moveVector += Physics.gravity;
        }

        //Apply our move Vector , remeber to multiply by Time.delta
        controller.Move(moveVector * Time.deltaTime);


    }

}