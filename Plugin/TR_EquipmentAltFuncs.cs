using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;
using Chabuk.ManikinMono;

namespace AcidSpell
{
    class TR_EquipmentAltFuncs
    {
        public static ContainerData.Content GetWornContent(Creature creature, string channel, int layer)
        {
            foreach (ContainerData.Content content in creature.container.contents.Where<ContainerData.Content>((Func<ContainerData.Content, bool>)(i => i.itemData != null && i.itemData.type == ItemData.Type.Wardrobe)))
            {
                ItemModuleWardrobe module = content.itemData.GetModule<ItemModuleWardrobe>();
                if (module != null)
                {
                    ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(creature);
                    if (wardrobe != null && wardrobe.manikinWardrobeData != null)
                    {
                        for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
                        {
                            if (wardrobe.manikinWardrobeData.channels[index] == channel && wardrobe.manikinWardrobeData.layers[index] == layer)
                                return content;
                        }
                    }
                }
            }
            return null;
        }

        public static void PrintWornContent(Creature c)
        {
            Logger.Detailed("Creature {0} ({1}, {2}) wears:", c.name, c.creatureId, c.GetInstanceID());
            foreach (string channel in new List<string> { "Head", "Torso", "HandLeft", "HandRight", "Legs", "Feet" })
            {
                foreach (int layer in Enumerable.Range(0, 15))
                {
                    var content = GetWornContent(c, channel, layer);
                    if (content != null)
                    {
                        Logger.Detailed("   {0}, {1} : {2}", channel, layer, content.itemData.id);
                    }
                }
            }
        }

        public static void RemoveWardrobeItem(Creature creature, ManikinLocations.LocationKey key)
        {
            Logger.Detailed("Removing manikin part: {0}, {1}", key.channel, key.layer);
            var content = GetWornContent(creature, key.channel, key.layer);
            if (content != null)
            {
                creature.equipment.UnequipWardrobe(content, false);
                ItemModuleWardrobe module = content.itemData.GetModule<ItemModuleWardrobe>();
                if (module != null)
                {
                    ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(creature);
                    creature.manikinParts.disableRenderersDuringUpdate = false;
                    bool flag = false;
                    for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
                    {
                        if (GetWornContent(creature, wardrobe.manikinWardrobeData.channels[index], wardrobe.manikinWardrobeData.layers[index]) != null)
                        {
                            creature.manikinLocations.RemovePart(wardrobe.manikinWardrobeData.channels[index], wardrobe.manikinWardrobeData.layers[index]);
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        creature.equipment.UpdateParts();
                    }
                }
            }
            else
            {
                RemoveUnderwear(creature, key);
            }
        }
        public static bool isUnderWear(ManikinLocations.LocationKey key)
        {
            return LUT.mapManikinChannel2UnderwearIndices[key.channel].Contains(key.layer);
        }

        public static void RemoveUnderwear(Creature creature, ManikinLocations.LocationKey key)
        {
            creature.manikinLocations.RemovePart(key);
        }
    }
}
