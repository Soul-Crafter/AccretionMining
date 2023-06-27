using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public struct inputData
    {
        public Vector3 moveVector;
        public Vector3 rotVector;
        public Vector3 mouseAxes;
        public float scrollAxis;
        public bool sprinting;
    }

    public struct moveData
    {
        public Vector3 moveVector;
        public Vector3 rotVector;
    }

    public struct prevState
    {
        public Vector3 prevMousePos;
    }

    public struct moveConsts
    {
        public float hSpeed;
        public float vSpeed;
        public float sprintSpeed;
        public float lookSpeed;
        public float scrollSpeed;
        public float dragSpeed;
        public GameObject player;
        public Rigidbody rb;
    }

    public enum PlayerType 
    {
        OmnipotentController,
        FPVController
    }


    public interface IPlayer
    {
        moveConsts mConsts { get; }
        PlayerType playerType { get; }
        inputData getInput();
        void move(moveData move);
        moveData fixedCalculateMove(inputData input);
        moveData updateCalculateMove(inputData input);
    }
}