using Balla.Input;
using UnityEngine;
namespace Balla.Core
{
    public interface IBallaMessages
    {
        /// <summary>
        /// Called in-line with Unity's Update. This is called every frame.
        /// </summary>
        public abstract void OnFrame();
        /// <summary>
        /// Called just after the frame. Similar to Unity's Late Update.
        /// </summary>
        public abstract void AfterFrame();
        /// <summary>
        /// Called every n seconds, determined by Time.FixedTimeStep. Called in-line with Unity's FixedUpdate.
        /// </summary>
        public abstract void Timestep();
    }
}
