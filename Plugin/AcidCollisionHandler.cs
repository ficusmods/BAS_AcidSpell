using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using ThunderRoad;
using Chabuk.ManikinMono;

namespace AcidSpell
{
    class AcidCollisionHandler : MonoBehaviour
    {
        public ParticleSystem part;
        public List<ParticleCollisionEvent> collisionEvents;

        static float hitEffectNextTime = 0.0f;
        static float hitEffectRevealNextTime = 0.0f;

        void Start()
        {
            part = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
        }
        private static void PlayBodyHitRevealEffect(Collider collider, Vector3 intersection, Vector3 normal)
        {
            if (Config.enableWoundDecals)
            {
                if (Time.time >= hitEffectRevealNextTime)
                {
                    EffectData hitEffectRevealData = Catalog.GetData<EffectData>("HitAcidSpellReveal");
                    ColliderGroup colliderGroup = collider.GetComponentInParent<ColliderGroup>();
                    var collisionInstance = new CollisionInstance(new DamageStruct(DamageType.Energy, 0.0f));
                    collisionInstance.targetColliderGroup = colliderGroup;
                    collisionInstance.contactPoint = intersection;
                    collisionInstance.contactNormal = normal;
                    EffectInstance hitEffectReveal = hitEffectRevealData.Spawn(intersection, Quaternion.LookRotation(normal), collider.transform, collisionInstance, true);
                    hitEffectReveal.Play();
                    hitEffectRevealNextTime = Time.time + 0.5f;
                }
            }
        }

        private static void PlayHitEffect(Collider collider, Vector3 intersection)
        {
            if (Config.enableEffects)
            {
                if (Time.time >= hitEffectNextTime)
                {
                    EffectData hitEffectData = Catalog.GetData<EffectData>("HitAcidSpell");
                    EffectInstance hitEffect = hitEffectData.Spawn(intersection, Quaternion.LookRotation(Vector3.up, Vector3.up), collider.transform, null, true);
                    hitEffect.Play();
                    hitEffectNextTime = Time.time + 1.2f;
                }
            }
        }

        void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

            int i = 0;
            while (i < numCollisionEvents)
            {
                Collider collider = collisionEvents[i].colliderComponent as Collider;
                RagdollPart rpart = collider.attachedRigidbody?.GetComponentInChildren<RagdollPart>();
                Item item = collider.attachedRigidbody?.GetComponentInChildren<Item>();
                if (rpart != null)
                {
                    AcidSpell.DamagePart(collider, rpart, Config.acidIntegrityDamage);
                    PlayBodyHitRevealEffect(collider, collisionEvents[i].intersection, collisionEvents[i].normal);
                    PlayHitEffect(collider, collisionEvents[i].intersection);
                    break;
                }
                else if(item != null)
                {
                    AcidSpell.DamageItem(collider, item, Config.acidIntegrityDamage);
                    PlayHitEffect(collider, collisionEvents[i].intersection);
                    break;
                }

                i++;
            }
        }
    }
}
