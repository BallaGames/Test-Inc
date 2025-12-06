using Balla.Input;
using UnityEngine;


namespace Balla.Core
{



    /// <summary>
    /// A <see cref="MonoBehaviour"/> that implements <see cref="IBallaMessages"/> (<see cref="IBallaMessages.OnFrame"/>, <see cref="IBallaMessages.AfterFrame"/> and <see cref="IBallaMessages.Timestep"/>).
    /// <br></br>Inheriting this makes it easier to implement other features and neatens code.
    /// <br></br>While the implementation of those messages introduces another layer of indirection, I do not think this is an issue, as we optimise the inital calls using delegates.<br></br>
    /// The only things on this component that cannot be overridden are the (Un)Subscribe methods, as these should generally not be tampered with.
    /// </summary>
    public class BallaScript : MonoBehaviour, IBallaMessages
    {
        /// <summary>
        /// The <see cref="GameCore"/> Singleton that controls these scripts.<br></br>
        /// This allows the scripts that inherit <see cref="BallaNetScript"/> or <see cref="BallaScript"/> to access several important things from the <see cref="GameCore"/>.<br></br>
        /// </summary>
        internal GameCore Core => GameCore.Core;
        /// <summary>
        /// The Fixed Delta Time from <see cref="GameCore"/>. This is just a shorthand accessor to GameCore.Delta. While it adds another layer of indirection, it neatens the code slightly.
        /// </summary>
        internal float Delta => GameCore.Delta;

        /// <summary>
        /// A shorthand accessor for the current <see cref="PlayerInput"/> singleton.
        /// </summary>
        internal PlayerInput Input => PlayerInput.InputManager;
        void Subscribe()
        {
            GameCore.Subscribe(this);
        }
        void Unsubscribe()
        {
            GameCore.Unsubscribe(this);
        }
        protected virtual void OnEnable()
        {
            Subscribe();
        }
        protected virtual void OnDisable()
        {
            Unsubscribe();
        }


        /// <summary>
        /// Called just after the frame. Similar to Unity's Late Update.
        /// </summary>
        protected virtual void AfterFrame()
        {

        }
        /// <summary>
        /// Called in-line with Unity's Update. This is called every frame.
        /// </summary>
        protected virtual void OnFrame()
        {

        }
        /// <summary>
        /// Called every n seconds, determined by Time.FixedTimeStep. Called in-line with Unity's FixedUpdate.
        /// </summary>
        protected virtual void Timestep()
        {

        }

        #region Interface Implementation
        void IBallaMessages.OnFrame()
        {
            OnFrame();
        }
        void IBallaMessages.AfterFrame()
        {
            AfterFrame();
        }
        void IBallaMessages.Timestep()
        {
            Timestep();
        }
        #endregion
    }
}