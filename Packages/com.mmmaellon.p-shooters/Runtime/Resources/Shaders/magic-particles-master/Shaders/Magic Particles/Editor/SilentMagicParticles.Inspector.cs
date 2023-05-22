using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using System.Linq;

// Parts of this file are based on https://github.com/Microsoft/MixedRealityToolkit-Unity/
// 	 Copyright (c) Microsoft Corporation. All rights reserved.
// 	 Licensed under the MIT License.

namespace SilentMagicParticles.Unity
{
	public class Inspector : ShaderGUI
	{
		public class SMPBoot : AssetPostprocessor {
			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths) {
			var isUpdated = importedAssets.Any(path => path.StartsWith("Assets/")) &&
							importedAssets.Any(path => path.Contains("SMP_InspectorData"));

			if (isUpdated) {
				InitializeOnLoad();
			}
			}

			[InitializeOnLoadMethod]
			private static void InitializeOnLoad() {
			Inspector.LoadInspectorData();
			}
		}

        public enum DepthWrite
        {
            Off,
            On
        }

        public enum RenderingMode
        {
            Opaque = 0,
            Cutout = 1,
            Fade = 2,
            Transparent = 3,
            Additive = 4,
            Custom = 5
        }

        public enum CustomRenderingMode
        {
            Opaque = 0,
            Cutout = 1,
            Fade = 2
        }

        protected static class BaseStyles
        {
            public static string renderTypeName = "RenderType";
            public static string renderingModeName = "_Mode";
            public static string customRenderingModeName = "_CustomMode";
            public static string sourceBlendName = "_ParticleSrcBlend";
            public static string destinationBlendName = "_ParticleDstBlend";
            public static string blendOperationName = "_BlendOp";
            public static string depthTestName = "_ZTest";
            public static string depthWriteName = "_ParticleZWrite";
            public static string colorWriteMaskName = "_ColorMask";

            public static string cullModeName = "_CullMode";
            public static string renderQueueOverrideName = "_RenderQueueOverride";

            public static string alphaToMaskName = "_AtoCMode";
            public static string alphaTestOnName = "_ALPHATEST_ON";
            public static string alphaBlendOnName = "_ALPHABLEND_ON";
            public static string alphaPremultiplyOnName = "_ALPHAPREMULTIPLY_ON";

            public static readonly string[] renderingModeNames = Enum.GetNames(typeof(RenderingMode));
            public static readonly string[] customRenderingModeNames = Enum.GetNames(typeof(CustomRenderingMode));
            public static readonly string[] depthWriteNames = Enum.GetNames(typeof(DepthWrite));

            public static string stencilComparisonName = "_StencilComp";
            public static string stencilOperationName = "_StencilOp";
            public static string stencilFailName = "_StencilFail";
            public static string stencilZFailName = "_StencilZFail";
        }

		static private TextAsset inspectorData;
		public static Dictionary<string, GUIContent> styles = new Dictionary<string, GUIContent>();

		protected MaterialProperty renderQueueOverride;

		protected GUIContent Content(string i)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				return style;
			} 
			return new GUIContent(i);
		}

		protected Material target;
		protected MaterialEditor editor;
		Dictionary<string, MaterialProperty> props = new Dictionary<string, MaterialProperty>();

		protected MaterialProperty Property(string i)
		{
			MaterialProperty prop;
			if (props.TryGetValue(i, out prop))
			{
				return prop;
			} 
			return new MaterialProperty();
		}

		public static void LoadInspectorData()
		{
			char[] recordSep = new char[] {'\n'};
			char[] fieldSep = new char[] {'\t'};
			//if (styles.Count == 0)
			{
					string[] guids = AssetDatabase.FindAssets("t:TextAsset SMP_InspectorData." + Application.systemLanguage);
					if (guids.Length == 0)
					{
						guids = AssetDatabase.FindAssets("t:TextAsset SMP_InspectorData.English");
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

		protected void FindProperties(MaterialProperty[] matProps, Material material)
		{ 	
			foreach (MaterialProperty prop in matProps)
			{
				props[prop.name] = FindProperty(prop.name, matProps, false);
			}
			renderQueueOverride = props[BaseStyles.renderQueueOverrideName];
		}
        
        protected bool initialised;

        protected void Initialise(Material material)
        {
            if (!initialised)
            {
                MaterialChanged(material);
                initialised = true;
            }
        }

        protected virtual void MaterialChanged(Material material)
        {
            SetupMaterialWithRenderingMode(material, 
                (RenderingMode)props[BaseStyles.renderingModeName].floatValue, 
                (CustomRenderingMode)props[BaseStyles.customRenderingModeName].floatValue, 
                (int)Property(BaseStyles.renderQueueOverrideName).floatValue);
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

		protected Rect TexturePropertySingleLine(string i)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				return editor.TexturePropertySingleLine(style, props[i]);
			} 
			return editor.TexturePropertySingleLine(new GUIContent(i), props[i]);
		}

		protected Rect TexturePropertySingleLine(string i, string i2)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				return editor.TexturePropertySingleLine(style, props[i], props[i2]);
			} 
			return editor.TexturePropertySingleLine(new GUIContent(i), props[i], props[i2]);
		}

		protected Rect TexturePropertySingleLine(string i, string i2, string i3)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				return editor.TexturePropertySingleLine(style, props[i], props[i2], props[i3]);
			} 
			return editor.TexturePropertySingleLine(new GUIContent(i), props[i], props[i2], props[i3]);
		}

		protected Rect TextureColorPropertyWithColorReset(string tex, string col)
		{
            bool hadTexture = props[tex].textureValue != null;
			Rect returnRect = TexturePropertySingleLine(tex, col);
			
            float brightness = props[col].colorValue.maxColorComponent;
            if (props[tex].textureValue != null && !hadTexture && brightness <= 0f)
                props[col].colorValue = Color.white;
			return returnRect;
		}

		protected Rect TextureColorPropertyWithColorReset(string tex, string col, string prop)
		{
            bool hadTexture = props[tex].textureValue != null;
			Rect returnRect = TexturePropertySingleLine(tex, col, prop);
			
            float brightness = props[col].colorValue.maxColorComponent;
            if (props[tex].textureValue != null && !hadTexture && brightness <= 0f)
                props[col].colorValue = Color.white;
			return returnRect;
		}

		protected Rect TexturePropertyWithHDRColor(string i, string i2)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				return editor.TexturePropertyWithHDRColor(style, props[i], props[i2], false);
			} 
			return editor.TexturePropertyWithHDRColor(new GUIContent(i), props[i], props[i2], false);
		}

		protected void ShaderProperty(string i)
		{
			GUIContent style;
			if (styles.TryGetValue(i, out style))
			{
				editor.ShaderProperty(props[i], style);
			} else {
				editor.ShaderProperty(props[i], new GUIContent(i));
			}
		}
		

        public static void Vector2Property(MaterialProperty property, GUIContent name)
        {
            EditorGUI.BeginChangeCheck();
            // Align to match scale/offset property
            float kLineHeight = 16;
            float kIndentPerLevel = 15;
            float kVerticalSpacingMultiField = 0;
            Vector2 propValue = new Vector2(property.vectorValue.x, property.vectorValue.y);
            Rect position = EditorGUILayout.GetControlRect(true, 2 * (kLineHeight + kVerticalSpacingMultiField), 
                EditorStyles.layerMaskField);
            float indent = EditorGUI.indentLevel * kIndentPerLevel;
            float labelWidth = EditorGUIUtility.labelWidth;
            float controlStartX = position.x + labelWidth;
            float labelStartX = position.x + indent;
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect labelRect = new Rect(labelStartX, position.y, labelWidth, kLineHeight);
            Rect valueRect = new Rect(controlStartX, position.y, position.width - labelWidth, kLineHeight);
            EditorGUI.PrefixLabel(labelRect, name);
            propValue = EditorGUI.Vector2Field(valueRect, GUIContent.none, propValue);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(propValue.x, propValue.y, property.vectorValue.z, property.vectorValue.w);
        }

        public static void Vector2PropertyZW(MaterialProperty property, GUIContent name)
        {
            EditorGUI.BeginChangeCheck();
            // Align to match scale/offset property
            float kLineHeight = 16;
            float kIndentPerLevel = 15;
            float kVerticalSpacingMultiField = 0;
            Vector2 propValue = new Vector2(property.vectorValue.x, property.vectorValue.y);
            Rect position = EditorGUILayout.GetControlRect(true, 2 * (kLineHeight + kVerticalSpacingMultiField), 
                EditorStyles.layerMaskField);
            float indent = EditorGUI.indentLevel * kIndentPerLevel;
            float labelWidth = EditorGUIUtility.labelWidth;
            float controlStartX = position.x + labelWidth;
            float labelStartX = position.x + indent;
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect labelRect = new Rect(labelStartX, position.y, labelWidth, kLineHeight);
            Rect valueRect = new Rect(controlStartX, position.y, position.width - labelWidth, kLineHeight);
            EditorGUI.PrefixLabel(labelRect, name);
            propValue = EditorGUI.Vector2Field(valueRect, GUIContent.none, propValue);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(property.vectorValue.x, property.vectorValue.y, propValue.x, propValue.y);
        }

        protected void DrawShaderPropertySameLine(string i) {
        	int HEADER_HEIGHT = 22; // Arktoon default
            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
            r.y -= HEADER_HEIGHT;
            r.height = MaterialEditor.GetDefaultPropertyHeight(props[i]);
            editor.ShaderProperty(r, props[i], " ");
        }

		public static class DefaultStyles
		{
			public static GUIStyle scmStyle;
			public static GUIStyle sectionHeader;
			public static GUIStyle sectionHeaderBox;
            static DefaultStyles()
            {
				scmStyle = new GUIStyle("DropDownButton");
				sectionHeader = new GUIStyle(EditorStyles.miniBoldLabel);
				sectionHeader.padding.left = 24;
				sectionHeader.padding.right = -24;
				sectionHeaderBox = new GUIStyle( GUI.skin.box );
				sectionHeaderBox.alignment = TextAnchor.MiddleLeft;
				sectionHeaderBox.padding.left = 5;
				sectionHeaderBox.padding.right = -5;
				sectionHeaderBox.padding.top = 0;
				sectionHeaderBox.padding.bottom = 0;
			}
		}

		protected Rect DrawSectionHeaderArea(GUIContent content)
		{
            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
				r.x -= 2.0f;
				r.y += 2.0f;
				r.height = 18.0f;
				r.width -= 0.0f;
			GUI.Box(r, EditorGUIUtility.IconContent("d_FilterByType"), DefaultStyles.sectionHeaderBox);
			EditorGUILayout.LabelField(content, DefaultStyles.sectionHeader);
			return r;
		}

		public class HeaderExDecorator : MaterialPropertyDrawer
    	{
	        private readonly string header;

	        public HeaderExDecorator(string header)
	        {
	            this.header = header;
	        }

	        // so that we can accept Header(1) and display that as text
	        public HeaderExDecorator(float headerAsNumber)
	        {
	            this.header = headerAsNumber.ToString();
	        }

	        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	        {
	            return 24f;
	        }

	        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	        {/*
	            position.y += 8;
	            position = EditorGUI.IndentedRect(position);
	            GUI.Label(position, header, EditorStyles.boldLabel);
*/
            Rect r = position;
				r.x -= 2.0f;
				r.y += 2.0f;
				r.height = 18.0f;
				r.width -= 0.0f;
			GUI.Box(r, EditorGUIUtility.IconContent("d_FilterByType"), DefaultStyles.sectionHeaderBox);
			position.y += 2;
			GUI.Label(position, header, DefaultStyles.sectionHeader);
	        }
    	}

		

// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/Inspector/MaterialEditor.cs#L1468
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] matProps)
		{ 
			this.target = materialEditor.target as Material;
			this.editor = materialEditor;
			Material material = this.target;
		
            FindProperties(matProps,material);

            DrawSectionHeaderArea(Content("s_mainOptions"));

			int propertyIndex = 0;
			foreach (MaterialProperty prop in matProps)
			{
				if (!ShaderUtil.IsShaderPropertyHidden(material.shader, propertyIndex)) 
				{
	                if (!styles.ContainsKey(prop.name) ) 
	                {
	                editor.ShaderProperty(prop, prop.displayName);
	                } else {
					ShaderProperty(prop.name);
	                }
				}
				propertyIndex++;
			}
			FooterOptions();
        }

        protected void RenderingModeOptions(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();

            MaterialProperty renderingMode = Property(BaseStyles.renderingModeName);
            EditorGUI.showMixedValue = renderingMode.hasMixedValue;
            RenderingMode mode = (RenderingMode)renderingMode.floatValue;
            EditorGUI.BeginChangeCheck();
            mode = (RenderingMode)EditorGUILayout.Popup(renderingMode.displayName, (int)mode, BaseStyles.renderingModeNames);

            if (EditorGUI.EndChangeCheck())
            {
                materialEditor.RegisterPropertyChangeUndo(renderingMode.displayName);
                renderingMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                Object[] targets = renderingMode.targets;

                foreach (Object target in targets)
                {
                    MaterialChanged((Material)target);
                }
            }

            if ((RenderingMode)renderingMode.floatValue == RenderingMode.Custom)
            {
                EditorGUI.indentLevel += 2;
                //customRenderingMode.floatValue = EditorGUILayout.Popup(customRenderingMode.displayName, (int)customRenderingMode.floatValue, BaseStyles.customRenderingModeNames);
                WithMaterialPropertyDropdown(Property(BaseStyles.customRenderingModeName), Enum.GetNames(typeof(CustomRenderingMode)), editor);
                ShaderProperty(BaseStyles.sourceBlendName);
                ShaderProperty(BaseStyles.destinationBlendName);
                ShaderProperty(BaseStyles.blendOperationName);
                ShaderProperty(BaseStyles.depthTestName);
                //depthWrite.floatValue = EditorGUILayout.Popup(depthWrite.displayName, (int)depthWrite.floatValue, BaseStyles.depthWriteNames);
                WithMaterialPropertyDropdown(Property(BaseStyles.depthWriteName), Enum.GetNames(typeof(DepthWrite)), editor);
                ShaderProperty(BaseStyles.colorWriteMaskName);
                EditorGUI.indentLevel -= 2;
            }

            //ShaderProperty(BaseStyles.cullModeName);
            WithMaterialPropertyDropdown(Property(BaseStyles.cullModeName), Enum.GetNames(typeof(UnityEngine.Rendering.CullMode)), editor);
        }

        protected static void SetupMaterialWithRenderingMode(Material material, RenderingMode mode, CustomRenderingMode customMode, int renderQueueOverride)
        {
            // If we aren't switching to Custom, then set default values for all RenderingMode types. Otherwise keep whatever user had before
            if (mode != RenderingMode.Custom)
            {
                material.SetInt(BaseStyles.blendOperationName, (int)BlendOp.Add);
                material.SetInt(BaseStyles.depthTestName, (int)CompareFunction.LessEqual);
                //material.SetFloat(BaseStyles.depthOffsetFactorName, 0.0f);
                //material.SetFloat(BaseStyles.depthOffsetUnitsName, 0.0f);
                material.SetInt(BaseStyles.colorWriteMaskName, (int)ColorWriteMask.All);
            }

            switch (mode)
            {
                case RenderingMode.Opaque:
                    {
                        material.SetOverrideTag(BaseStyles.renderTypeName, BaseStyles.renderingModeNames[(int)RenderingMode.Opaque]);
                        material.SetInt(BaseStyles.customRenderingModeName, (int)CustomRenderingMode.Opaque);
                        material.SetInt(BaseStyles.sourceBlendName, (int)BlendMode.One);
                        material.SetInt(BaseStyles.destinationBlendName, (int)BlendMode.Zero);
                        material.SetInt(BaseStyles.blendOperationName, (int)BlendOp.Add);
                        material.SetInt(BaseStyles.depthTestName, (int)CompareFunction.LessEqual);
                        material.SetInt(BaseStyles.depthWriteName, (int)DepthWrite.On);
                        material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.colorWriteMaskName, (int)ColorWriteMask.All);
                        material.DisableKeyword(BaseStyles.alphaTestOnName);
                        material.DisableKeyword(BaseStyles.alphaBlendOnName);
                        material.DisableKeyword(BaseStyles.alphaPremultiplyOnName);
                        material.renderQueue = (renderQueueOverride >= 0) ? renderQueueOverride : (int)RenderQueue.Geometry;
                    }
                    break;

                case RenderingMode.Cutout:
                    {
                        material.SetOverrideTag(BaseStyles.renderTypeName, BaseStyles.renderingModeNames[(int)RenderingMode.Cutout]);
                        material.SetInt(BaseStyles.customRenderingModeName, (int)CustomRenderingMode.Cutout);
                        material.SetInt(BaseStyles.sourceBlendName, (int)BlendMode.One);
                        material.SetInt(BaseStyles.destinationBlendName, (int)BlendMode.Zero);
                        material.SetInt(BaseStyles.blendOperationName, (int)BlendOp.Add);
                        material.SetInt(BaseStyles.depthTestName, (int)CompareFunction.LessEqual);
                        material.SetInt(BaseStyles.depthWriteName, (int)DepthWrite.On);
                        material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.On);
                        material.SetInt(BaseStyles.colorWriteMaskName, (int)ColorWriteMask.All);
                        material.EnableKeyword(BaseStyles.alphaTestOnName);
                        material.DisableKeyword(BaseStyles.alphaBlendOnName);
                        material.DisableKeyword(BaseStyles.alphaPremultiplyOnName);
                        material.renderQueue = (renderQueueOverride >= 0) ? renderQueueOverride : (int)RenderQueue.AlphaTest;
                    }
                    break;

                case RenderingMode.Fade:
                    {
                        material.SetOverrideTag(BaseStyles.renderTypeName, BaseStyles.renderingModeNames[(int)RenderingMode.Fade]);
                        material.SetInt(BaseStyles.customRenderingModeName, (int)CustomRenderingMode.Fade);
                        material.SetInt(BaseStyles.sourceBlendName, (int)BlendMode.SrcAlpha);
                        material.SetInt(BaseStyles.destinationBlendName, (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt(BaseStyles.blendOperationName, (int)BlendOp.Add);
                        material.SetInt(BaseStyles.depthTestName, (int)CompareFunction.LessEqual);
                        material.SetInt(BaseStyles.depthWriteName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.colorWriteMaskName, (int)ColorWriteMask.All);
                        material.DisableKeyword(BaseStyles.alphaTestOnName);
                        material.EnableKeyword(BaseStyles.alphaBlendOnName);
                        material.DisableKeyword(BaseStyles.alphaPremultiplyOnName);
                        material.renderQueue = (renderQueueOverride >= 0) ? renderQueueOverride : (int)RenderQueue.Transparent;
                    }
                    break;

                case RenderingMode.Transparent:
                    {
                        material.SetOverrideTag(BaseStyles.renderTypeName, BaseStyles.renderingModeNames[(int)RenderingMode.Fade]);
                        material.SetInt(BaseStyles.customRenderingModeName, (int)CustomRenderingMode.Fade);
                        material.SetInt(BaseStyles.sourceBlendName, (int)BlendMode.One);
                        material.SetInt(BaseStyles.destinationBlendName, (int)BlendMode.OneMinusSrcAlpha);
                        material.SetInt(BaseStyles.blendOperationName, (int)BlendOp.Add);
                        material.SetInt(BaseStyles.depthTestName, (int)CompareFunction.LessEqual);
                        material.SetInt(BaseStyles.depthWriteName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.colorWriteMaskName, (int)ColorWriteMask.All);
                        material.DisableKeyword(BaseStyles.alphaTestOnName);
                        material.EnableKeyword(BaseStyles.alphaBlendOnName);
                        material.DisableKeyword(BaseStyles.alphaPremultiplyOnName);
                        material.renderQueue = (renderQueueOverride >= 0) ? renderQueueOverride : (int)RenderQueue.Transparent;
                    }
                    break;

                case RenderingMode.Additive:
                    {
                        material.SetOverrideTag(BaseStyles.renderTypeName, BaseStyles.renderingModeNames[(int)RenderingMode.Fade]);
                        material.SetInt(BaseStyles.customRenderingModeName, (int)CustomRenderingMode.Fade);
                        material.SetInt(BaseStyles.sourceBlendName, (int)BlendMode.One);
                        material.SetInt(BaseStyles.destinationBlendName, (int)BlendMode.One);
                        material.SetInt(BaseStyles.blendOperationName, (int)BlendOp.Add);
                        material.SetInt(BaseStyles.depthTestName, (int)CompareFunction.LessEqual);
                        material.SetInt(BaseStyles.depthWriteName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.Off);
                        material.SetInt(BaseStyles.colorWriteMaskName, (int)ColorWriteMask.All);
                        material.DisableKeyword(BaseStyles.alphaTestOnName);
                        material.EnableKeyword(BaseStyles.alphaBlendOnName);
                        material.DisableKeyword(BaseStyles.alphaPremultiplyOnName);
                        material.renderQueue = (renderQueueOverride >= 0) ? renderQueueOverride : (int)RenderQueue.Transparent;
                    }
                    break;

                case RenderingMode.Custom:
                    {
                        material.SetOverrideTag(BaseStyles.renderTypeName, BaseStyles.customRenderingModeNames[(int)customMode]);
                        // _SrcBlend, _DstBlend, _BlendOp, _ZTest, _ZWrite, _ColorWriteMask are controlled by UI.

                        switch (customMode)
                        {
                            case CustomRenderingMode.Opaque:
                                {
                                    material.DisableKeyword(BaseStyles.alphaTestOnName);
                                    material.DisableKeyword(BaseStyles.alphaBlendOnName);
                                    material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.Off);
                                }
                                break;

                            case CustomRenderingMode.Cutout:
                                {
                                    material.EnableKeyword(BaseStyles.alphaTestOnName);
                                    material.DisableKeyword(BaseStyles.alphaBlendOnName);
                                    material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.On);
                                }
                                break;

                            case CustomRenderingMode.Fade:
                                {
                                    material.DisableKeyword(BaseStyles.alphaTestOnName);
                                    material.EnableKeyword(BaseStyles.alphaBlendOnName);
                                    material.SetInt(BaseStyles.alphaToMaskName, (int)DepthWrite.Off);
                                }
                                break;
                        }

                        material.renderQueue = (renderQueueOverride >= 0) ? renderQueueOverride : material.renderQueue;
                    }
                    break;
            }

            // If Stencil is set to NotEqual, raise the queue by 1.
            if (material.GetInt("_StencilComp") == (int)CompareFunction.NotEqual)
            {
                material.renderQueue += 1;
            }
        }

		protected void FooterOptions()
		{
			EditorGUILayout.Space();

			if (WithChangeCheck(() => 
			{
				editor.ShaderProperty(renderQueueOverride, Content(BaseStyles.renderQueueOverrideName));
			})) {
				MaterialChanged(target);
			}

			// Show the RenderQueueField but do not allow users to directly manipulate it. That is done via the renderQueueOverride.
			GUI.enabled = false;
			editor.RenderQueueField();

			if (!GUI.enabled && !target.enableInstancing)
			{
				target.enableInstancing = true;
			}

			editor.EnableInstancingField();
		}
    }
}
