using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ThunderRoad;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.AddressableAssets;
using Chabuk.ManikinMono;

namespace AcidSpell
{
    class LUT
    {
        public static Dictionary<RagdollPart.Type, List<string>> mapRagdollPart2ManikinChannels = new Dictionary<RagdollPart.Type, List<string>>()
        {
            { RagdollPart.Type.Head, new List<string>{ "Head" } },
            { RagdollPart.Type.Neck, new List<string>{ "Head" } },
            { RagdollPart.Type.Torso, new List<string>{ "Torso", "HandLeft", "HandRight" } },
            { RagdollPart.Type.LeftArm, new List<string>{ "Torso", "HandLeft" } },
            { RagdollPart.Type.LeftHand, new List<string>{ "HandLeft" } },
            { RagdollPart.Type.LeftLeg, new List<string>{ "Legs" } },
            { RagdollPart.Type.LeftFoot, new List<string>{ "Legs", "Feet", } },
            { RagdollPart.Type.RightArm, new List<string>{ "Torso", "HandRight" } },
            { RagdollPart.Type.RightHand, new List<string>{ "HandRight" } },
            { RagdollPart.Type.RightLeg, new List<string>{ "Legs", "Feet" } },
            { RagdollPart.Type.RightFoot, new List<string>{ "Feet" } },
        };

        public static Dictionary<string, Tuple<int, int>> mapManikinChannel2ArmorLayerRange = new Dictionary<string, Tuple<int, int>>()
        {
            // The ranges are inclusive
            { "Head", new Tuple<int, int>(0,4) },
            { "Torso", new Tuple<int, int>(0,3) },
            { "Legs", new Tuple<int, int>(0,3) },
            { "HandLeft", new Tuple<int, int>(0,3) },
            { "HandRight", new Tuple<int, int>(0,3) },
            { "Feet", new Tuple<int, int>(0,2) }
        };

        public static Dictionary<string, HashSet<int>> mapManikinChannel2UnderwearIndices = new Dictionary<string, HashSet<int>>()
        {
            { "Head", new HashSet<int>{ } },
            { "Torso", new HashSet<int>{ 3 } },
            { "HandLeft", new HashSet<int>{ 1 } },
            { "HandRight", new HashSet<int>{ 1 } },
            { "Legs", new HashSet<int>{ 3 } },
            { "Feet", new HashSet<int>{ } }
        };

        public static Dictionary<string, HashSet<string>> mapManikinChannel2UnequipExclusionSet = new Dictionary<string, HashSet<string>>()
        {
            { "Head", new HashSet<string>{ } },
            { "Torso", new HashSet<string>{ "Legs" } },
            { "HandLeft", new HashSet<string>{ } },
            { "HandRight", new HashSet<string>{ } },
            { "Legs", new HashSet<string>{ "Torso" } },
            { "Feet", new HashSet<string>{ } }
        };
    }
}
