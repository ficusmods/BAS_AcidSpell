using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;
using Chabuk.ManikinMono;

namespace AcidSpell
{
    class AcidSpell : SpellCastCharge
    {
        EffectInstance effectFlow;

        static Dictionary<Creature, Dictionary<ManikinLocations.LocationKey, float>> creatureIntegrityMap = new Dictionary<Creature, Dictionary<ManikinLocations.LocationKey, float>>();
        static Dictionary<Item, float> itemIntegrityMap = new Dictionary<Item, float>();
        static Dictionary<Creature, HashSet<ManikinLocations.LocationKey>> creatureRemovedPartsMap = new Dictionary<Creature, HashSet<ManikinLocations.LocationKey>>();

        public override void Init()
        {
            base.Init();
            EventManager.onCreatureSpawn += EventManager_onCreatureSpawn;
        }

        private void EventManager_onCreatureSpawn(Creature creature)
        {
            if (!creature.isPlayer)
            {
                Logger.Detailed("Creature {0} ({1}, {2}) spawned", creature.name, creature.creatureId, creature.GetInstanceID());
                resetIntegrityForCreature(creature);
                creature.equipment.EquipAllWardrobes(false, false);
                TR_EquipmentAltFuncs.PrintWornContent(creature);

                var roomChangeWatcher = creature.gameObject.AddComponent<CreatureRoomChangeWatcher>();
                roomChangeWatcher.onRoomChanged += RoomChangeWatcher_onRoomChanged;


                void DespawnHandler(EventTime t)
                {
                    if (t == EventTime.OnStart)
                    {
                        Logger.Detailed("Creature {0} ({1}, {2}) despawned", creature.name, creature.creatureId, creature.GetInstanceID());
                        resetIntegrityForCreature(creature);
                        creature.equipment.EquipAllWardrobes(false, false);
                    }
                
                }
                creature.OnDespawnEvent -= DespawnHandler;
                creature.OnDespawnEvent += DespawnHandler;
            }
        }

        private void RoomChangeWatcher_onRoomChanged(Creature c, Room r)
        {
            foreach(ManikinLocations.LocationKey key in creatureRemovedPartsMap[c])
            {
                TR_EquipmentAltFuncs.RemoveWardrobeItem(c, key);
            }
        }

        public override void OnSprayStart()
        {
            base.OnSprayStart();
            LayerMask collisionLayer =
               1 << GameManager.GetLayer(LayerName.Default)
             | 1 << GameManager.GetLayer(LayerName.NPC)
             | 1 << GameManager.GetLayer(LayerName.BodyLocomotion)
             | 1 << GameManager.GetLayer(LayerName.MovingItem)
             | 1 << GameManager.GetLayer(LayerName.ItemAndRagdollOnly)
             | 1 << GameManager.GetLayer(LayerName.Ragdoll);

            EffectData flowEffectData = Catalog.GetData<EffectData>("AcidFlow");
            this.effectFlow = flowEffectData.Spawn(this.spellCaster.magic.position, this.spellCaster.magic.rotation, this.spellCaster.magic, null, true);
            foreach (Effect effect in this.effectFlow.effects)
            {
                effect.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.LiquidFlow));
                EffectParticle effectParticle = effect as EffectParticle;
                if (effectParticle != null)
                {
                    effectParticle.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    effectParticle.transform.localPosition += new Vector3(0.0f, 0.2f, 0.0f);
                    var particleSystem = effectParticle.rootParticleSystem;
                    var shape = particleSystem.shape;
                    shape.enabled = true;
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.arc = 360.0f;
                    shape.angle = 0.0f;
                    shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
                    shape.radius = 0.05f;
                    shape.length = 1.0f;

                    ParticleSystem.MainModule mmodule = particleSystem.main;
                    mmodule.startSpeed = 3.5f;
                    
                    foreach (EffectParticleChild child in effectParticle.childs)
                    {
                        particleSystem.gameObject.AddComponent<AcidCollisionHandler>();
                        var collision = particleSystem.collision;
                        collision.collidesWith = collisionLayer;
                    }
                }
            }
            this.effectFlow.Play();
        }

        public override void Fire(bool active)
        {
            base.Fire(active);
            if (!active)
            {
                if(this.effectFlow != null)
                {
                    this.effectFlow.End();
                }
            }
            this.isSpraying = false;
        }

        public override void OnSprayStop()
        {
            base.OnSprayStop();
            Logger.Detailed("OnSprayStop");
            this.effectFlow.End();
        }

        private static void resetIntegrityForCreature(Creature creature)
        {
            creatureIntegrityMap[creature] = new Dictionary<ManikinLocations.LocationKey, float>();
            creatureRemovedPartsMap[creature] = new HashSet<ManikinLocations.LocationKey>();
            Item leftItem = creature.handLeft?.grabbedHandle?.item;
            Item rightItem = creature.handRight?.grabbedHandle?.item;
            if(leftItem !=null) itemIntegrityMap.Remove(leftItem);
            if(rightItem != null) itemIntegrityMap.Remove(rightItem);
        }

        private static void DamagePartIntegrity(Creature creature, ManikinLocations.LocationKey key, float dmg)
        {
            var integrityMap = creatureIntegrityMap[creature];
            if (!integrityMap.ContainsKey(key))
            {
                integrityMap[key] = 1.0f;
            }

            if (integrityMap[key] < Config.integrityThreshold && !creatureRemovedPartsMap[creature].Contains(key))
            {
                creatureRemovedPartsMap[creature].Add(key);
                TR_EquipmentAltFuncs.RemoveWardrobeItem(creature, key);
                creature.manikinLocations.UpdateParts();
                creature.UpdateRenderers();
            }
            else
            {
                if (integrityMap[key] > 0)
                {
                    integrityMap[key] -= dmg;
                    Logger.Detailed("Damaging manikin part: {0}, {1}, integrity: {2}", key.channel, key.layer, integrityMap[key]);
                }
            }
        }

        private static void DamageItemIntegrity(Item item, float dmg)
        {
            if (itemIntegrityMap[item] < Config.integrityThreshold)
            {
                itemIntegrityMap.Remove(item);
                item.mainHandler.TryRelease();
            }
            else
            {
                itemIntegrityMap[item] -= dmg;
                Logger.Detailed("Damaging item: {0} ({1}, {2}) integrity: {3}", item.name, item.itemId, item.GetInstanceID(), itemIntegrityMap[item]);
            }
        }

        public static void DamagePart(RagdollPart rpart, float dmg)
        {
            Creature creature = rpart.ragdoll.creature;
            if (creature.isPlayer) return;

            List<string> channels = LUT.mapRagdollPart2ManikinChannels[rpart.type];
            foreach (string channel in channels)
            {
                Tuple<int, int> armorIndexes = LUT.mapManikinChannel2ArmorLayerRange[channel];
                int armorIndex;
                for (armorIndex = armorIndexes.Item1; armorIndex <= armorIndexes.Item2; armorIndex++)
                {
                    var key = new ManikinLocations.LocationKey(channel, armorIndex);
                    ManikinPart mpart = creature.manikinLocations.GetPartAtLocation(key);

                    if (mpart != null)
                    {
                        if (!TR_EquipmentAltFuncs.isUnderWear(key) || Config.dissolveUnderwear)
                        {
                            DamagePartIntegrity(creature, key, dmg);
                        }
                        break;
                    }
                }
            }

            if(rpart.type == RagdollPart.Type.LeftHand || rpart.type == RagdollPart.Type.RightHand)
            {
                DamageItem(creature.handLeft?.grabbedHandle?.item, dmg);
            }
            else if (rpart.type == RagdollPart.Type.RightHand)
            {
                DamageItem(creature.handRight?.grabbedHandle?.item, dmg);
            }
        }

        public static void DamageItem(Item item, float dmg)
        {
            if (item != null)
            {
                if (item.handlers.Count > 0)
                {
                    if (!item.mainHandler.ragdoll.creature.isPlayer)
                    {
                        if (!itemIntegrityMap.ContainsKey(item))
                        {
                            itemIntegrityMap.Add(item, 1.0f);
                        }
                        DamageItemIntegrity(item, Config.acidIntegrityDamage);
                    }
                }
            }
        }
    }
}
