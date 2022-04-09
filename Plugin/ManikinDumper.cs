using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ThunderRoad;
using Chabuk.ManikinMono;
using UnityEngine;

namespace AcidSpell
{
    public class ManikinDumper
    {
        /* ***************************************************************************** */
        /* ***************************************************************************** */
        /* ***************************************************************************** */
        /* ***************************************************************************** */
        /*                                                                               */
        /* This code is no longer used, just here for safe-keeping and looking up things */
        /* ***************************************************************************** */
        /* ***************************************************************************** */
        /* ***************************************************************************** */
        /* ***************************************************************************** */

        public static Dictionary<string, ManikinLocations.LocationKey> mapPartName2ManikinKey = new Dictionary<string, ManikinLocations.LocationKey>()
        {
            // Head channel
            /* Head armor goes to layers [0,4] */
            { "BaseBeard", new ManikinLocations.LocationKey("Head", 5)},
            { "BaseHair", new ManikinLocations.LocationKey("Head", 6)},
            { "BaseEyesBrow", new ManikinLocations.LocationKey("Head", 8)},
            { "BaseEyesLashes", new ManikinLocations.LocationKey("Head", 9)},
            { "BaseEyes", new ManikinLocations.LocationKey("Head", 10)},
            { "BaseMouth", new ManikinLocations.LocationKey("Head", 11)},
            { "BaseHead", new ManikinLocations.LocationKey("Head", 12)},

            // Torso channel
            /* Torso armor goes to layers [0,2] */
            { "BaseBra", new ManikinLocations.LocationKey("Torso", 3)},
            { "BaseTorso", new ManikinLocations.LocationKey("Torso", 7)},

            // Legs channel
            /* Legs armor goes to layers [0,2] */
            { "BaseUnderwear", new ManikinLocations.LocationKey("Legs", 3)},
            { "BaseLegs", new ManikinLocations.LocationKey("Legs", 5)},

            // HandRight channel
            { "BaseGloveRight", new ManikinLocations.LocationKey("HandRight", 0)},
            { "BaseWristRight", new ManikinLocations.LocationKey("HandRight", 1)},
            /* Additional armor goes to layers [2,3] */
            { "BaseHandRight", new ManikinLocations.LocationKey("HandRight", 4)},

            // HandLeft channel
            { "BaseGloveLeft", new ManikinLocations.LocationKey("HandLeft", 0)},
            { "BaseWristLeft", new ManikinLocations.LocationKey("HandLeft", 1)},
            /* Additional armor goes to layers [2,3] */
            { "BaseHandLeft", new ManikinLocations.LocationKey("HandLeft", 4)},

            // Feet channel
            /* Feet armor goes to layers [0,2] */
            { "BaseFeet", new ManikinLocations.LocationKey("Feet", 3)},

        };

        public static Dictionary<string, List<string>> mapManikinChannel2RelevantPartNames = new Dictionary<string, List<string>>()
        {
            { "Head", new List<string>(){ "BaseBeard", "BaseHair", "BaseEyesBrow", "BaseEyesLashes", "BaseEyes", "BaseMouth", "BaseHead" } },
            { "Torso", new List<string>(){ "BaseBra", "BaseTorso" } },
            { "HandLeft", new List<string>(){ "BaseGloveLeft", "BaseWristLeft", "BaseHandLeft" } },
            { "HandRight", new List<string>(){ "BaseGloveRight", "BaseWristRight", "BaseHandRight" } },
            { "Legs", new List<string>(){ "BaseUnderwear", "BaseLegs" } },
            { "Feet", new List<string>(){ "BaseUnderwear", "BaseLegs" } },
        };

        static int json_location_count = 0;
        public static void dumpJSONManikinLocations(ManikinLocations.JsonWardrobeLocations data)
        {
            ManikinLocations ml = new ManikinLocations();
            ml.FromJson(data);

            string path = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, $"AcidSpell/Dumps/json_manikin_locations_{json_location_count}.json");
            json_location_count++;
            //var jsondata = UnityEngine.JsonUtility.ToJson(data.wardrobe, true);
            var jsondata = ml.ToJson(true);
            if (File.Exists(path)) File.Delete(path);
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, jsondata);
        }

        class DumpableWData
        {
            public string assetPrefabName;
            public string[] channels;
            public int[] layers;
            public int[] fullyOccludedLayers;
            public int[] partialOccludedLayers;
            public int[] partialOccludedMasks;
            public string occlusionID;
            public int occlusionIDHash;
            public string[] tags;
        }

        static int location_count = 0;
        public static void dumpManikinLocations(ManikinLocations data)
        {
            string path = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, $"AcidSpell/Dumps/manikin_locations_{location_count}.json");
            location_count++;
            List<DumpableWData> dumpabeList = new List<DumpableWData>();
            foreach(ManikinWardrobeData wdata in data.partLocations.Values)
            {
                DumpableWData dwdata = new DumpableWData();
                dwdata.assetPrefabName = wdata.assetPrefab.AssetGUID;
                dwdata.channels = wdata.channels;
                dwdata.layers = wdata.layers;
                dwdata.fullyOccludedLayers = wdata.fullyOccludedLayers;
                dwdata.partialOccludedLayers = wdata.partialOccludedLayers;
                dwdata.partialOccludedMasks = wdata.partialOccludedMasks;
                dwdata.occlusionID = wdata.occlusionID;
                dwdata.occlusionIDHash = wdata.occlusionIDHash;
                dwdata.tags = wdata.channels;
                dumpabeList.Add(dwdata);
            }

            var jsondata = Newtonsoft.Json.JsonConvert.SerializeObject(dumpabeList, Newtonsoft.Json.Formatting.Indented);
            if (File.Exists(path)) File.Delete(path);
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, jsondata);
        }

        public static void dumpCreatureManikinData(Creature c)
        {
            string path = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, $"AcidSpell/Dumps/manikin_data_{location_count}.json");
            location_count++;

            ManikinAvatar mavatar = c.GetComponentInChildren<ManikinAvatar>();
            ManikinRig mrig = c.GetComponentInChildren<ManikinRig>();

            List<DumpableWData> dumpabeList = new List<DumpableWData>();
            foreach (ManikinWardrobeData wdata in c.manikinLocations.partLocations.Values)
            {
                DumpableWData dwdata = new DumpableWData();
                dwdata.assetPrefabName = wdata.assetPrefab.AssetGUID;
                dwdata.channels = wdata.channels;
                dwdata.layers = wdata.layers;
                dwdata.fullyOccludedLayers = wdata.fullyOccludedLayers;
                dwdata.partialOccludedLayers = wdata.partialOccludedLayers;
                dwdata.partialOccludedMasks = wdata.partialOccludedMasks;
                dwdata.occlusionID = wdata.occlusionID;
                dwdata.occlusionIDHash = wdata.occlusionIDHash;
                dwdata.tags = wdata.channels;
                dumpabeList.Add(dwdata);
            }

            Dictionary<string, ManikinBoneOverrideAsset> manikinBoneOverrideAssets = new Dictionary<string, ManikinBoneOverrideAsset>();
            foreach(ManikinPart mpart in c.manikinLocations.PartList.GetComponents<ManikinPart>())
            {
                if (mpart.TryGetComponent<ManikinBoneOverride>(out ManikinBoneOverride mbo) && mbo.boneOverrideAsset != null)
                {
                    manikinBoneOverrideAssets.Add(mpart.name, mbo.boneOverrideAsset);
                    mbo.boneOverrideAsset.OnBeforeSerialize();
                }
            }
            c.manikinLocations.OnBeforeSerialize();
            mavatar.OnBeforeSerialize();
            mrig.OnBeforeSerialize();

            Dictionary<string, object> jsonRoot = new Dictionary<string, object>()
            {
                {"ManikinAvatar", mavatar},
                {"ManikinRig", mrig},
                {"WardrobeData", dumpabeList},
                {"BoneOverrideAssets", manikinBoneOverrideAssets}
            };

            var jsondata = Newtonsoft.Json.JsonConvert.SerializeObject(jsonRoot, Newtonsoft.Json.Formatting.Indented);
            if (File.Exists(path)) File.Delete(path);
            if (!Directory.Exists(Path.GetDirectoryName(path))) Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, jsondata);
        }
    }
}
