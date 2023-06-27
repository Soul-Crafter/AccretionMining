using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        // Globals //
        IPlayer player;

        inputData _input;
        inputData Input
        {
            get => _input;
            set { _input = value; }
        }

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
            
            player = new OmnipotentController(
                new moveConsts
                {
                    hSpeed = hSpeed,
                    lookSpeed = lookSpeed,
                    sprintSpeed = sprintSpeed,
                    player = gameObject,
                    vSpeed = vSpeed,
                    scrollSpeed = scrollSpeed,
                    dragSpeed = dragSpeed,
                    rb = gameObject.GetComponent<Rigidbody>()
                }
            );
            
            

        }

        // Update is called once per frame
        void Update()
        {
            _input = player.getInput();
            player.move(player.updateCalculateMove(_input));
        }

        void FixedUpdate()
        {
            player.move(player.fixedCalculateMove(_input));
        }

    }
}