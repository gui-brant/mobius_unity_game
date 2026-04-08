using UnityEngine;

namespace Combat
{
    public class ComplacencyProjectile : Projectile
    {
        private float slowMultiplier = 0.4f;
        private float slowDuration = 2f;

        // Called by ComplacencyBoss immediately after ProjectileSpawner returns this instance
        public void SetSlowParameters(float multiplier, float duration)
        {
            slowMultiplier = multiplier;
            slowDuration   = duration;
        }

        // Base class handles damage, team filtering, and owner filtering before calling this
        protected override void HandleTargetHit(GameObject targetObject)
        {
            base.HandleTargetHit(targetObject); // Deal damage as normal

            // Walk the same hierarchy the base class uses, so slow always finds ISlowable
            ISlowable slowable = GetInterfaceFromBehaviours<ISlowable>(targetObject.GetComponents<MonoBehaviour>());

            if (slowable == null)
            {
                Transform current = targetObject.transform.parent;
                while (current != null && slowable == null)
                {
                    slowable = GetInterfaceFromBehaviours<ISlowable>(current.GetComponents<MonoBehaviour>());
                    current  = current.parent;
                }
            }

            slowable?.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}