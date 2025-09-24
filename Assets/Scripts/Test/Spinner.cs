using UnityEngine;
using Balla.Core;
namespace Balla
{
    public class Spinner : BallaScript
    {
        public Vector3 axis;
        public float speed;

        protected override void Timestep()
        {
            transform.Rotate(Delta * speed * axis);
        }
    }
}
