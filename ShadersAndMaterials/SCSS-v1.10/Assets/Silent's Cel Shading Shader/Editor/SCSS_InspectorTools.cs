using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Linq;

// Parts of this file are based on https://github.com/Microsoft/MixedRealityToolkit-Unity/
// licensed under the MIT license. 

namespace SilentCelShading.Unity
{
	public class InspectorCommon
	{
		public enum OutlineMode
		{
			None,
			Tinted,
			Colored
		}

		public enum AlbedoAlphaMode
		{
			Transparency,
			Smoothness,
			ClippingMask
		}

		public enum SpecularType
		{
			Disable,
			Standard,
			Cloth,
			Anisotropic,
			Cel,
			CelStrand
		}

		public enum LightingCalculationType
		{
			Unbiased = 0,
			Standard = 1,
			Cubed = 2,
			Directional = 3, 
			Biased = 4
		}

		public enum AmbientFresnelType
		{
			Disable,
			Lit,
			Ambient,
			AmbientAlt
		}

		public enum MatcapBlendModes
		{
			Additive,
			Multiply,
			Median,
		}

		public enum MatcapType
		{
			Disable,
			Standard,
			Anisotropic
		}

		public enum VertexColorType
		{
			Color = 0,
			OutlineColor = 1,
			AdditionalData = 2,
			Ignore = 3
		}

		public static class CommonStyles
		{
			public static string albedoMapAlphaSmoothnessName = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
			public static readonly string[] albedoAlphaModeNames = Enum.GetNames(typeof(AlbedoAlphaMode));
		}

		public class SCSSBoot : AssetPostprocessor {
			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths) {
			var isUpdated = importedAssets.Any(path => path.StartsWith("Assets/")) &&
							importedAssets.Any(path => path.Contains("SCSS_InspectorData"));

			if (isUpdated) {
				InitializeOnLoad();
			}
			}

			[InitializeOnLoadMethod]
			private static void InitializeOnLoad() {
			InspectorCommon.LoadInspectorData();
			}
		}

		static private TextAsset inspectorData;
		public static Dictionary<string, GUIContent> styles = new Dictionary<string, GUIContent>();

		public static void LoadInspectorData()
		{
			char[] recordSep = new char[] {'\n'};
			char[] fieldSep = new char[] {'\t'};
			//if (styles.Count == 0)
			{
					string[] guids = AssetDatabase.FindAssets("t:TextAsset SCSS_InspectorData." + Application.systemLanguage);
					if (guids.Length == 0)
					{
						guids = AssetDatabase.FindAssets("t:TextAsset SCSS_InspectorData.English");
					}
					inspectorData = (TextAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(TextAsset));

				string[] records = inspectorData.text.Split(recordSep, System.StringSplitOptions.RemoveEmptyEntries);
				foreach (string record in records)
				{
					string[] fields = record.Split(fieldSep, 3, System.StringSplitOptions.None); 
					if (fields.Length != 3) {Debug.LogWarning("Field " + fields[0] + " only has " + fields.Length + " fields!");};
					if (fields[0] != null) styles[fields[0]] = new GUIContent(fields[1], fields[2]);  
					
				}	
			}		
		}

        public static void WithGroupVertical(Action action)
        {
            EditorGUILayout.BeginVertical();
            action();
            EditorGUILayout.EndVertical();
        }

		// Warning: Do not use BeginHorizontal with ShaderProperty because it causes issues with the layout.
        public static void WithGroupHorizontal(Action action)
        {
            EditorGUILayout.BeginHorizontal();
            action();
            EditorGUILayout.EndHorizontal();
        }

		public static bool WithChangeCheck(Action action)
		{
			EditorGUI.BeginChangeCheck();
			action();
			return EditorGUI.EndChangeCheck();
		}

		public static void WithGUIDisable(bool disable, Action action)
		{
			bool prevState = GUI.enabled;
			GUI.enabled = disable;
			action();
			GUI.enabled = prevState;
		}

		public static Material[] WithMaterialPropertyDropdown(MaterialProperty prop, string[] options, MaterialEditor editor)
		{
			int selection = (int)prop.floatValue;
			EditorGUI.BeginChangeCheck();
			selection = EditorGUILayout.Popup(prop.displayName, (int)selection, options);

			if (EditorGUI.EndChangeCheck())
			{
				editor.RegisterPropertyChangeUndo(prop.displayName);
				prop.floatValue = (float)selection;
				return Array.ConvertAll(prop.targets, target => (Material)target);
			}

			return new Material[0];

		}
		
		public static Material[] WithMaterialPropertyDropdownNoLabel(MaterialProperty prop, string[] options, MaterialEditor editor)
		{
			int selection = (int)prop.floatValue;
			EditorGUI.BeginChangeCheck();
			selection = EditorGUILayout.Popup((int)selection, options);

			if (EditorGUI.EndChangeCheck())
			{
				editor.RegisterPropertyChangeUndo(prop.displayName);
				prop.floatValue = (float)selection;
				return Array.ConvertAll(prop.targets, target => (Material)target);
			}

			return new Material[0];

		}
    }
}