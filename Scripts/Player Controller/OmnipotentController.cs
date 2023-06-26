using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class OmnipotentController : MonoBehaviour, IPlayer
    {

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
            inputData input;
            input.moveVector = new Vector3();
            input.rotVector = new Vector3();

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

            return input;
        }
        

        public moveData calculateMove(inputData input, moveConsts mConsts)
        {
            moveData move = new moveData();

            // Horizontal Movement
            move.moveVector += input.moveVector.z * mConsts.player.transform.forward;
            move.moveVector += input.moveVector.x * mConsts.player.transform.right;
            move.moveVector = move.moveVector.normalized * mConsts.hSpeed * Time.deltaTime;
            move.moveVector *= input.sprinting ? mConsts.sprintSpeed : 1;
            
            // Vertical Movement
            move.moveVector.y = input.moveVector.y * mConsts.vSpeed * Time.deltaTime;

            // Lateral Rotation
            move.rotVector = input.rotVector * Time.deltaTime * mConsts.lookSpeed;

            // Dragging Movement
            if(Input.GetKey(KeyCode.Mouse2))
            {
                move.moveVector -= Quaternion.AngleAxis(mConsts.player.transform.eulerAngles.y, Vector3.up) * getMouseDrag2() * mConsts.dragSpeed;
            }
            

            /*
            // Scroll move add
            Vector3 tempScrollZoom = scrollZoom(input, mConsts);
            //Debug.Log(tempScrollZoom);
            move.moveVector += tempScrollZoom;
            */
            

            return move;
        }


        Vector3 scrollZoom(inputData input, moveConsts mConsts)
        {
                return (cursorWorldPosOnNCP - mConsts.player.transform.position) 
                        * Input.GetAxis("Mouse ScrollWheel") * mConsts.scrollSpeed;
        }


        /*
        Vector3 grabDrag(inputData input, moveConsts mConsts)
        {
            // Direction to move based on user's dragging input //

            Vector3 worldCursorPos = getCursorInWorld(mConsts);

            if (Input.GetKey(KeyCode.Mouse2))
                return new Vector3(worldCursorPos.x - _prevState.prevMousePos.x, 0, worldCursorPos.z - _prevState.prevMousePos.z);

            // Updating prevState for the next frame. //
            prevStateMousePos = worldCursorPos;

            return Vector3.zero;

        }

        Vector3 getCursorInWorld(moveConsts mConsts)
        {
            Vector3 NCPdir = cursorWorldPosOnNCP;
            Vector3 position = mConsts.player.transform.position;
            return new Vector3(position.x - (position.y * NCPdir.x / NCPdir.y),
                               0,
                               position.z - (position.y * NCPdir.z / NCPdir.y));
        }
        */


        private bool _isDragging = false;
        bool IsDragging
        {
            get => _isDragging;
            set => _isDragging = value;
        }

        private Vector3 _dragOrigin = Vector3.zero;
        Vector3 DragOrigin
        {
            get => _dragOrigin;
            set => _dragOrigin = value;
        }

        Vector3 GetMouseDrag(moveConsts mConsts)
        {

            switch (IsDragging, Input.GetKey(KeyCode.Mouse2))
            {
                case (false, true):
                    (IsDragging, DragOrigin) = (true, GetMouseOnHorizontalPlane());
                    return Vector3.zero;

                case (true, false):
                    IsDragging = false;
                    return Vector3.zero;

                case (true, true): spawnCube(DragOrigin); return (DragOrigin - GetMouseOnHorizontalPlane()) * mConsts.dragSpeed;

                default: return Vector3.zero;
            };
        }



        Vector3 GetMouseOnHorizontalPlane()
        {
            Vector3 mouseOnNearClip = cursorWorldPosOnNCP;

            Vector3 cameraPos = Camera.main.transform.position;

            Vector3 cameraToMouseDirection = Camera.main.ScreenPointToRay(mouseOnNearClip).direction;

            // Maybe
            // if cameraPos.y < 0 then we return vector3.zero
            // if cameraToMouseDirection.y < 0 then we return vector3.zero
            // Since inputs might be shit

            return
                cameraPos +
                    (-(5) / cameraToMouseDirection.y)
                    * cameraToMouseDirection;

            
        }

        Vector3 getMouseDrag2 ()
        {
            return new Vector3(Input.GetAxisRaw("Mouse X"), 0, Input.GetAxisRaw("Mouse Y"));
        }

        /*
        // Position Lerp - Handles scroll and grab n' drag
        IEnumerator positionLerp(Rigidbody rb, inputData input, moveConsts mConsts)
        {
            // get direction to move for scroll //
            // lerp towards final scroll position //

            Vector3 scrollDir = scrollZoom(input, mConsts);
            
        }
        */


        public void move(Rigidbody rb, moveData move, moveConsts mConsts)
        {
            rb.MovePosition(mConsts.player.transform.position + move.moveVector);
            rb.MoveRotation(Quaternion.Euler(mConsts.player.transform.rotation.eulerAngles + move.rotVector));

            DragOrigin = GetMouseOnHorizontalPlane();
        }


        void spawnCube(Vector3 position)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Instantiate(cube, position, Quaternion.identity);
        }
        
    }

}


