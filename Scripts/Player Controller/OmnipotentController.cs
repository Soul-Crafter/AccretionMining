using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class OmnipotentController : MonoBehaviour, IPlayer
    {
        moveConsts _mConsts;

        public moveConsts mConsts
        {
            get => _mConsts;
            private set { _mConsts = value; }
        }

        
        public OmnipotentController(moveConsts mConsts)
        {
            this.mConsts = mConsts;
        }


        // Globals :sob:
        public prevState _prevState;

        public Vector3 prevStateMousePos
        {
            get => _prevState.prevMousePos;
            set
            {
                _prevState.prevMousePos = value;
            }
        }

        public GameObject cube;
        

        private static Vector3 cursorWorldPosOnNCP
        {
            get
            {
                return Camera.main.ScreenToWorldPoint(
                    new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y,
                    Camera.main.nearClipPlane));
            }
        }


        public PlayerType playerType { get { return PlayerType.OmnipotentController; } }


        public inputData getInput()
        {

            inputData input = new inputData();

            // Displacement
            input.moveVector = new Vector3(Input.GetAxisRaw("Horizontal"),
                                     0,
                                     Input.GetAxisRaw("Vertical"));

            // Rotation
            input.rotVector = new Vector3(0, Input.GetAxisRaw("Yaw"), 0);

            // Sprint
            if (Input.GetKey(KeyCode.LeftShift)) input.sprinting = true;
            else input.sprinting = false;

            // Vertical movement
            input.moveVector.y = Input.GetAxisRaw("Elevate");

            // Scroll Axis
            input.scrollAxis = Input.GetAxis("Mouse ScrollWheel");

            // Grab and Drag
            input.mouseAxes = new Vector3(Input.GetAxisRaw("Mouse X"), 0, Input.GetAxisRaw("Mouse Y"));

            return input;
        }
        

        public moveData fixedCalculateMove(inputData input)
        {
            moveData move = new moveData();

            // Horizontal Movement
            move.moveVector += input.moveVector.z * mConsts.player.transform.forward;
            move.moveVector += input.moveVector.x * mConsts.player.transform.right;
            move.moveVector = move.moveVector.normalized * mConsts.hSpeed;
            move.moveVector *= input.sprinting ? mConsts.sprintSpeed : 1;
            
            // Vertical Movement
            move.moveVector.y = input.moveVector.y * mConsts.vSpeed;

            // Lateral Rotation
            move.rotVector = input.rotVector * mConsts.lookSpeed;

            // Dragging Movement
            if(Input.GetKey(KeyCode.Mouse2))
            {
                move.moveVector -= Quaternion.AngleAxis(mConsts.player.transform.eulerAngles.y, Vector3.up)
                                                        * (input.mouseAxes + new Vector3(0, 0, input.mouseAxes.y * input.mouseAxes.y))
                                                        * mConsts.dragSpeed * mConsts.player.transform.position.y;
            }


            return move;
        }

        public moveData updateCalculateMove(inputData input)
        {
            moveData move = new moveData();

            // Scroll zooming
            move.moveVector += scrollZoom(input);

            return move;
        }

        Vector3 scrollZoom(inputData input)
        {

            return (cursorWorldPosOnNCP - mConsts.player.transform.position).normalized 
                    * Input.GetAxis("Mouse ScrollWheel") * mConsts.scrollSpeed;
        }

        public void move(moveData move)
        {
            
            mConsts.player.transform.position = (mConsts.player.transform.position + move.moveVector);
            mConsts.player.transform.rotation = (Quaternion.Euler(mConsts.player.transform.rotation.eulerAngles + move.rotVector));
        }


        void spawnCube(Vector3 position)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Instantiate(cube, position, Quaternion.identity);
        }
        
    }

}


