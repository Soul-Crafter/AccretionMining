using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        // Globals //
        Rigidbody rb;
        IPlayer player;
        moveConsts mConsts;

        [Header("Omnipotent Controller Options")]
        [SerializeField] float hSpeed;
        [SerializeField] float lookSpeed;
        [SerializeField] float sprintSpeed;
        [SerializeField] float vSpeed;
        [SerializeField] float scrollSpeed;
        [SerializeField] float dragSpeed;

        // Start is called before the first frame update
        void Start()
        {
            // Define globals //
            rb = gameObject.GetComponent<Rigidbody>();
            player = new OmnipotentController();

            mConsts = new moveConsts
            {
                hSpeed = hSpeed,
                lookSpeed = lookSpeed,
                sprintSpeed = sprintSpeed,
                player = gameObject,
                vSpeed = vSpeed,
                scrollSpeed = scrollSpeed,
                dragSpeed = dragSpeed
            };
            
        }

        // Update is called once per frame
        void Update()
        {
            player.move(rb, player.calculateMove(player.getInput(), mConsts), mConsts);
        }

    }
}