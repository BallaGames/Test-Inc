
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement Configs")]
    [SerializeField] private float walkSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 10.0f;
    [SerializeField] private float jumpHeight = 20.0f; 
    private Rigidbody rb;
    private CharacterController characterController; //Yes I'm using CharacterControllers Lunar, Bite Me >:P
    private Vector2 moveDirection = Vector2.zero;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        characterController.Move(new Vector3(moveDirection.x, 0, moveDirection.y)*Time.deltaTime*walkSpeed); //Need to multiply this by the forward direction when we get camera controls :D
    }
    private void FixedUpdate()
    {
       
    }
    public void UpdateMovement(InputAction.CallbackContext callbackContext)
    {
     
        moveDirection=callbackContext.ReadValue<Vector2>();

    }
}
