using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool disabled = false;
    
    public Transform vrPlayer;                  
    private CharacterController cc;            

    public float lookDownAngle = 25.0f;       
    public float speed = 3.0f;                 
    public bool moveForward;                    




    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();       

    }

    // Update is called once per frame
    void Update()
    {

        if(!disabled)
        {
            UpdateMovement();
        }

    }

    void UpdateMovement()
    {

        if (vrPlayer.eulerAngles.x >= lookDownAngle && vrPlayer.eulerAngles.x < 90.0f)
        {            // Check if the VRPlayers headmovement rotation is more than the lookDownAngle and less than the floor
            moveForward = true;                                                                     // Switch Boolean to being in the moveForward state
        }
        else
        {
            moveForward = false;                                                                    // If the above IF statment is not true then the player's Booelan moveForward is set to to False
        }

        if (moveForward == true)
        {                                                                  // What to do if the moveForward state is True                                                    
            Vector3 forward = vrPlayer.TransformDirection(Vector3.forward);                         // Create a Vector3 variable called Forward and apply a conversion to it so movement will work correctly

            cc.SimpleMove(forward * speed);                                                         // Tell the Character Controller for the Player GameObject to move in the way of the above Forward variable and multiply it be speed

        }
    }



}
