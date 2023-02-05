using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.CharacterSpeech;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace MithrixBleed
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "MithrixSaysBleed";
        public const string PluginVersion = "1.0.0";

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            On.RoR2.CharacterSpeech.CharacterSpeechController.SpeakNow += CharacterSpeechController_SpeakNow;

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        static void CharacterSpeechController_SpeakNow(On.RoR2.CharacterSpeech.CharacterSpeechController.orig_SpeakNow orig, CharacterSpeechController self, ref CharacterSpeechController.SpeechInfo speechInfo)
        {
            try
            {
                if (NetworkServer.active && !string.IsNullOrEmpty(speechInfo.token) && Language.GetString(speechInfo.token, "en").ToLower().Contains("bleed"))
                {
                    GameObject mithrixBodyObject = CharacterBody.readOnlyInstancesList.FirstOrDefault(cb => cb.bodyIndex == BodyCatalog.FindBodyIndex("BrotherBody") || cb.bodyIndex == BodyCatalog.FindBodyIndex("BrotherHurtBody"))?.gameObject;

                    foreach (TeamComponent playerTeamMember in TeamComponent.GetTeamMembers(TeamIndex.Player))
                    {
                        if (!playerTeamMember)
                            continue;

                        CharacterBody playerTeamBody = playerTeamMember.body;
                        if (!playerTeamBody)
                            continue;

                        DotController.InflictDot(playerTeamBody.gameObject, mithrixBodyObject, DotController.DotIndex.Bleed);
                    }
                }
            }
            finally
            {
                orig(self, ref speechInfo);
            }
        }
    }
}
