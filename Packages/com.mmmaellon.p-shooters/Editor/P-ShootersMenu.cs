#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;


namespace MMMaellon.P_Shooters
{
    [InitializeOnLoad]
    public class P_ShootersMenu : IVRCSDKBuildRequestedCallback
    {
        private const string RootMenu = "P-Shooters/";
        private const string MenuName = RootMenu + "Auto Setup";
        public static bool autoSetup = true;
        static P_ShootersMenu()
        {
            autoSetup = EditorPrefs.GetBool(MenuName, true);
        }

        [MenuItem(MenuName)]
        private static void ToggleAction()
        {
            autoSetup = !autoSetup;
            EditorPrefs.SetBool(MenuName, autoSetup);
        }

        [MenuItem(MenuName, true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(MenuName, autoSetup);
            return true;
        }


        [MenuItem(RootMenu + "Wiki (open in browser)")]
        static void OpenWiki()
        {
            Application.OpenURL("https://github.com/MMMaellon/P-Shooters/wiki");
        }

        [MenuItem(RootMenu + "Open Prefabs Folder")]
        private static void NavigateToPrefabs()
        {
            // Get a reference to your prefabs folder
            var prefabFolder = AssetDatabase.LoadAssetAtPath("Packages/com.mmmaellon.p-shooters/Runtime/Prefabs", typeof(Object));

            // If the folder exists, select it and focus the project window
            if (prefabFolder != null)
            {
                EditorUtility.FocusProjectWindow();
                AssetDatabase.OpenAsset(prefabFolder);
            }
            else
            {
                Debug.LogError("The specified prefab folder could not be found.");
            }
        }







        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            Setup();
        }

        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return Setup();
        }

        public static bool Setup()
        {
            if (!autoSetup)
            {
                return true;
            }
            foreach (HealthAndShieldChanger changer in GameObject.FindObjectsOfType<HealthAndShieldChanger>())
            {
                HealthAndShieldChanger.SetupHealthAndShieldChanger(changer);
            }
            foreach (AmmoTracker ammo in GameObject.FindObjectsOfType<AmmoTracker>())
            {
                AmmoTracker.SetupAmmoTracker(ammo);
            }
            foreach (Mag mag in GameObject.FindObjectsOfType<Mag>())
            {
                Mag.SetupMag(mag);
            }
            foreach (MagReload magReload in GameObject.FindObjectsOfType<MagReload>())
            {
                MagReload.SetupMagReload(magReload);
            }
            foreach (MagReceiver magReceiver in GameObject.FindObjectsOfType<MagReceiver>())
            {
                MagReceiver.SetupMagReceiver(magReceiver);
            }
            foreach (P_Shooter shooter in GameObject.FindObjectsOfType<P_Shooter>())
            {
                P_Shooter.SetupShooter(shooter);
            }
            foreach (RapidFire rapid in GameObject.FindObjectsOfType<RapidFire>())
            {
                RapidFire.SetupRapidFire(rapid);
            }
            foreach (Scope scope in GameObject.FindObjectsOfType<Scope>())
            {
                Scope.SetupScope(scope);
            }
            foreach (ParticleCollisionSFX sfx in GameObject.FindObjectsOfType<ParticleCollisionSFX>())
            {
                ParticleCollisionSFX.SetupParticleCollisionSFX(sfx);
            }

            P_ShootersPlayerHandler.SetupPlayers();

            return true;
        }
    }
}
#endif