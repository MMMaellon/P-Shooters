#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using System.IO;


namespace MMMaellon.P_Shooters
{
    [InitializeOnLoad]
    public class P_ShootersMenu : IVRCSDKBuildRequestedCallback
    {
        public const string RootMenu = "P-Shooters/";
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


        [MenuItem(RootMenu + "Trigger Auto Setup")]
        static void TriggerAutoSetup()
        {
            Setup();
        }

        [MenuItem(RootMenu + "Import Example Project")]
        public static void ImportExampleProject()
        {
            string sourceDir = "Packages/com.mmmaellon.p-shooters/Samples~/Example";
            // Show a folder panel to let the user choose the destination directory
            string destDir = EditorUtility.OpenFolderPanel("Select Destination Folder", "Assets", "");

            // Check if user pressed cancel on the dialog
            if (string.IsNullOrEmpty(destDir))
            {
                return;
            }

            // Ensure the destination directory is under the Assets folder
            if (!destDir.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Invalid destination folder",
                    "The destination folder must be inside the Assets folder.",
                    "OK");
                return;
            }

            // Convert full path to a relative path
            destDir = "Assets" + destDir.Substring(Application.dataPath.Length);


            // Check if directory already exists.
            if (Directory.Exists(destDir) && (new DirectoryInfo(destDir)).GetFiles().Length > 0)
            {
                // Ask the user if they want to overwrite the existing directory.
                bool overwrite = EditorUtility.DisplayDialog(
                    "Overwrite existing project?",
                    "The directory " + destDir + " already exists. Do you want to overwrite it?",
                    "Yes", "No");

                // If the user clicked "No", return without doing anything.
                if (!overwrite)
                    return;
            }
            // Copy the directory
            Copy(sourceDir, destDir);

            // Refresh the AssetDatabase after the copy
            AssetDatabase.Refresh();

            var exampleFolder = AssetDatabase.LoadAssetAtPath(destDir, typeof(Object));
            if (exampleFolder != null)
            {
                EditorUtility.FocusProjectWindow();
                AssetDatabase.OpenAsset(exampleFolder);
            }
        }

        [MenuItem(RootMenu + "Wiki (open in browser)")]
        static void OpenWiki()
        {
            Application.OpenURL("https://github.com/MMMaellon/P-Shooters/wiki");
        }

        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (!Directory.Exists(target.FullName))
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
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
            if (!autoSetup)
            {
                return;
            }
            Setup();
        }

        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (!autoSetup)
            {
                return true;
            }
            return Setup();
        }

        public static bool Setup()
        {
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
            foreach (AltFire rapid in GameObject.FindObjectsOfType<AltFire>())
            {
                AltFire.SetupRapidFire(rapid);
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