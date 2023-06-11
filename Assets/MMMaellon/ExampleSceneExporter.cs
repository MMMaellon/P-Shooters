#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


namespace MMMaellon.P_Shooters
{
    [InitializeOnLoad]
    public class ExampleSceneExporter
    {
        [MenuItem(P_ShootersMenu.RootMenu + "~~EXPORT EXAMPLE SCENE~~")]
        static void Export()
        {
            string sourceDir = "Assets/MMMaellon/P-Shooters2/";
            string tempDir = "Assets/MMMaellon/Temp/";
            string exampleDir = "Packages/com.mmmaellon.p-shooters/Samples~/Example/";
            // Save the current scene
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            string prevScene = EditorSceneManager.GetActiveScene().path;

            P_ShootersMenu.Copy(sourceDir, tempDir);
            // Refresh the AssetDatabase after the copy
            AssetDatabase.Refresh();

            string scenePath = tempDir + "ExampleScene.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (PrefabUtility.IsOutermostPrefabInstanceRoot(obj))
                {
                    PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }
            }
            // Save the current scene
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            P_ShootersMenu.Copy(tempDir, exampleDir);
            // Refresh the AssetDatabase after the copy
            AssetDatabase.Refresh();

            ClearDirectory(tempDir);
            DirectoryInfo dir = new DirectoryInfo(tempDir);
            dir.Delete();
            // Refresh the AssetDatabase after the copy
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(prevScene, OpenSceneMode.Single);
        }
        public static void ClearDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                ClearDirectory(subDir.FullName);
                subDir.Delete(true);
            }
        }
    }
}
#endif