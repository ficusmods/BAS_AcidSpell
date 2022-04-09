using System;
using System.Collections;
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
    public class LoadModule : LevelModule
    {

        public string mod_version = "0.0";
        public string mod_name = "UnnamedMod";
        public string logger_level = "Basic";

        public bool DissolveUnderwear
        {
            get => Config.dissolveUnderwear;
            set => Config.dissolveUnderwear = value;
        }
        public float AcidDamage
        {
            get => Config.acidDamage;
            set => Config.acidDamage = value;
        }
        public float AcidIntegrityDamage
        {
            get => Config.acidIntegrityDamage;
            set => Config.acidIntegrityDamage = value;
        }

        public float AcidIntegrityThreshold
        {
            get => Config.integrityThreshold;
            set => Config.integrityThreshold = value;
        }

        public bool EnableWoundDecals
        {
            get => Config.enableWoundDecals;
            set => Config.enableWoundDecals = value;
        }

        public bool EnableEffects
        {
            get => Config.enableEffects;
            set => Config.enableEffects = value;
        }

        public override IEnumerator OnLoadCoroutine()
        {
            Logger.init(mod_name, mod_version, logger_level);
            Logger.Basic("Loading " + mod_name);
            return base.OnLoadCoroutine();
        }
    }
}
