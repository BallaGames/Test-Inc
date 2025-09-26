using Balla.Core;
using Balla.Input;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Balla.Input
{
    /// <summary>
    /// A singleton <see cref="BallaScript"/> that processes input for the player.
    /// <br></br>This script does not implement any of the networked input logic.
    /// <br></br>This script implements logic for when inputs are lost/changed and when the game is unfocused.
    /// <br></br><see cref="PlayerInput"/> should not be manually added to a component, as it is added to the GameCore on initialisation.
    /// <para></para>I made the choice to implement input listeners as generics aside from those that I expect to have specific behaviours, such as pausing.
    /// </summary>
    public class PlayerInput : BallaScript
    {
        /// <summary>
        /// An instance of the script version of the Input Action Asset, which allows the use of the C# event bindings instead of the silly UnityEVent bindings.
        /// </summary>
        public CS_Actions actions;
        /// <summary>
        /// The currently active PlayerInput
        /// </summary>
        public static PlayerInput InputManager;

        internal Vector2 moveInput, lookInput;
        internal bool jumpInput, crouchInput, sprintInput, interactInput;
        public float lookSpeed = 15;

        public void Initialised()
        {
            actions = new CS_Actions();
            actions.Enable();
            SubscribeInput(actions.Player.Move, GetMove);
            SubscribeInput(actions.Player.Look, GetLook);
            SubscribeInput(actions.Player.Jump, GetJump);
            SubscribeInput(actions.Player.Interact, GetInteract);
            SubscribeInput(actions.Player.Crouch, GetCrouch);
            SubscribeInput(actions.Player.Sprint, GetSprint);
        }
        public void Terminate()
        {
            UnsubscribeInput(actions.Player.Move, GetMove);
            UnsubscribeInput(actions.Player.Look, GetLook);
            UnsubscribeInput(actions.Player.Jump , GetJump);
            UnsubscribeInput(actions.Player.Interact , GetInteract);
            UnsubscribeInput(actions.Player.Crouch , GetCrouch);
            UnsubscribeInput (actions.Player.Sprint , GetSprint);
            actions.Disable();
            actions.Dispose();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            actions?.Enable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            actions.Disable();
        }
        /// <summary>
        /// Starts listening to the <see cref="InputAction"/> via the getter method.
        /// <br></br>Adds the getter method as a listener to the <see cref="InputAction"/>'s performed and canceled events.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to start listening to.</param>
        /// <param name="getter">The method used to get the input value. This is added to the <see cref="InputAction"/>'s performed and canceled events.</param>
        public void SubscribeInput(InputAction action, Action<InputAction.CallbackContext> getter)
        {
            Debug.Log($"Subscribed {action.name} to {nameof(getter.Target)}");
            action.performed += getter;
            action.canceled += getter;
        }
        /// <summary>
        /// Stops listening to the <see cref="InputAction"/> via the getter method.
        /// <br></br>Removes the getter method as a listener to the <see cref="InputAction"/>'s performed and canceled events.
        /// </summary>
        /// <param name="action">The <see cref="InputAction"/> to stop listening to.</param>
        /// <param name="getter">The method used to get the input value. This is removed from the <see cref="InputAction"/>'s performed and canceled events.</param>
        public void UnsubscribeInput(InputAction action, Action<InputAction.CallbackContext> getter)
        {
            Debug.Log($"Unsubscribed {action.name} from {nameof(getter.Target)}");
            action.performed -= getter;
            action.canceled -= getter;
        }
        #region Input Callbacks
        public void GetMove(InputAction.CallbackContext ctx)
        {
            moveInput = ctx.ReadValue<Vector2>();
        }
        public void GetLook(InputAction.CallbackContext ctx)
        {
            //Look input is multiplied by delta time and lookSpeed when obtained
            lookInput = GameCore.TimeMultiplier * Time.deltaTime * lookSpeed * ctx.ReadValue<Vector2>();
        }
        public void GetInteract(InputAction.CallbackContext ctx)
        {
            interactInput = ctx.ReadValueAsButton();
        }
        public void GetCrouch(InputAction.CallbackContext ctx)
        {
            crouchInput = ctx.ReadValueAsButton();
        }
        public void GetJump(InputAction.CallbackContext ctx)
        {
            jumpInput = ctx.ReadValueAsButton();
        }
        public void GetSprint(InputAction.CallbackContext ctx)
        {
            sprintInput = ctx.ReadValueAsButton();
        }

        #endregion
    }
}
