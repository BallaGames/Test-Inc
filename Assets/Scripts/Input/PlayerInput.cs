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

        public static Action<bool> OnPauseChanged;

        internal Vector2 Move => GetValue(move);
        internal Vector2 Look => GetValue(look);
        internal bool Jump
        {
            get
            {
                return GetValue(jump);
            }
            set
            {
                jump = value;
            }
        }
        internal bool Crouch => GetValue(crouch);
        internal bool Sprint => GetValue(sprint);
        internal bool Interact => GetValue(interact);
        internal bool Attack => GetValue(attack);
        internal bool AltAttack => GetValue(altAttack);
        internal bool Grab => GetValue(grab);
        internal bool PrevEquip => GetValue(prevEquip);
        internal bool NextEquip => GetValue(nextEquip);
        Vector2 move, look;
        bool crouch, sprint, interact, attack, altAttack, grab, prevEquip, nextEquip, jump;

        bool GetValue(bool target)
        {
            return target && !gamePaused;
        }
        Vector2 GetValue(Vector2 target)
        {
            return gamePaused ? Vector2.zero : target;
        }

        public bool gamePaused;
        public float lookSpeed = 15;

        public Action<bool> OnPause;

        public void SubscribeToActionPerform(InputAction action, Action target)
        {
            action.performed += (ctx) => target?.Invoke();
        }
        public void UnsubscribeFromActionPerform(InputAction action, Action target)
        {
            action.performed -= (ctx) => target?.Invoke();
        }
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
            SubscribeInput(actions.Player.Attack, GetAttack);
            SubscribeInput(actions.Player.AltAttack, GetAltAttack);
            SubscribeInput(actions.Player.Grab, GetGrab);
            SubscribeInput(actions.Player.Next, GetNextEquip);
            SubscribeInput(actions.Player.Previous, GetPrevEquip);
            SubscribeInput(actions.Player.Pause, GetPause);
        }

        public void Terminate()
        {
            UnsubscribeInput(actions.Player.Move, GetMove);
            UnsubscribeInput(actions.Player.Look, GetLook);
            UnsubscribeInput(actions.Player.Jump, GetJump);
            UnsubscribeInput(actions.Player.Interact, GetInteract);
            UnsubscribeInput(actions.Player.Crouch, GetCrouch);
            UnsubscribeInput(actions.Player.Sprint, GetSprint);
            UnsubscribeInput(actions.Player.Attack, GetAttack);
            UnsubscribeInput(actions.Player.AltAttack, GetAltAttack);
            UnsubscribeInput(actions.Player.Grab, GetGrab);
            UnsubscribeInput(actions.Player.Next, GetNextEquip);
            UnsubscribeInput(actions.Player.Previous, GetPrevEquip);
            UnsubscribeInput(actions.Player.Pause, GetPause);
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
            move = ctx.ReadValue<Vector2>();
        }
        public void GetLook(InputAction.CallbackContext ctx)
        {
            //Look input is multiplied by delta time and lookSpeed when obtained
            look = GameCore.TimeMultiplier * Time.deltaTime * lookSpeed * ctx.ReadValue<Vector2>();
        }
        public void GetInteract(InputAction.CallbackContext ctx)
        {
            interact = ctx.ReadValueAsButton();
        }
        public void GetCrouch(InputAction.CallbackContext ctx)
        {
            crouch = ctx.ReadValueAsButton();
        }
        public void GetJump(InputAction.CallbackContext ctx)
        {
            jump = ctx.ReadValueAsButton();
        }
        public void GetSprint(InputAction.CallbackContext ctx)
        {
            sprint = ctx.ReadValueAsButton();
        }
        public void GetAttack(InputAction.CallbackContext ctx)
        {
            attack = ctx.ReadValueAsButton();
        }
        public void GetAltAttack(InputAction.CallbackContext ctx)
        {
            altAttack = ctx.ReadValueAsButton();
        }
        public void GetGrab(InputAction.CallbackContext ctx)
        {
            grab = ctx.ReadValueAsButton();
        }
        void GetNextEquip(InputAction.CallbackContext ctx)
        {
            nextEquip = ctx.ReadValueAsButton();
        }
        void GetPrevEquip(InputAction.CallbackContext ctx)
        {
            prevEquip = ctx.ReadValueAsButton();
        }
        void GetPause(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                TogglePause();
            }
        }
        #endregion

        void TogglePause()
        {
            SetPause(!gamePaused);
        }
        public void SetPause(bool paused)
        {
            gamePaused = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
            OnPause?.Invoke(paused);
            OnPauseChanged?.Invoke(paused);
        }

    }
}
