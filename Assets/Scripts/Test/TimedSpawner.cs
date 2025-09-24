using UnityEngine;
using Balla.Core;
namespace Balla.Utils
{
    public class TimedSpawner : BallaScript
    {
        public float interval;
        float timer;
    
        public SpriteRenderer bar;
        public GameObject prefab;
        public float maxScaleX;
        public float maxOffsetX;
        protected override void Timestep()
        {
            base.Timestep();
            timer += Delta;
            while (timer > interval)
            {
                timer %= interval;
                Instantiate(prefab, transform.position, transform.rotation);
            }
            float lerp = Mathf.InverseLerp(0, interval, timer);
            bar.transform.localScale = new(maxScaleX * lerp, 1, 1);
            bar.transform.localPosition = new(maxOffsetX * lerp, 0, 0);
        }
    }
}
