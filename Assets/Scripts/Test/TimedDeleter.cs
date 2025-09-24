using UnityEngine;
using Balla.Core;
namespace Balla.Utils
{
    public class TimedDeleter : BallaScript
    {
        private void Start()
        {
            timer = destroyTime;
        }
        public float destroyTime;
        float timer;
        protected override void Timestep()
        {
            timer += Delta;
            if (timer <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}