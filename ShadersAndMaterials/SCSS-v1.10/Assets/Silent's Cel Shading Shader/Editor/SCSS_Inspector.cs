using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using static SilentCelShading.Unity.InspectorCommon;
using SilentCelShading.Unity.Baking;

// Parts of this file are based on https://github.com/Microsoft/MixedRealityToolkit-Unity/
//	Copyright (c) Microsoft Corporation. All rights reserved.
//	Licensed under the MIT License.

namespace SilentCelShading.Unity
{
	public class Inspector : SCSSShaderGUI
	{

		public enum ShadowMaskType
		{
			Occlusion,
			Tone,
			Auto
		}

		public enum LightRampType
		{
			Horizontal,
			Vertical,
			None
		}

		public enum ToneSeparationType
		{
			Combined,
			Separate
		}

		public enum IndirectShadingType
		{
			Dynamic,
			Directional,
			Flatten
		}

		public enum SettingsComplexityMode
		{
			Complex,
			Normal,
			Simple
		}

		public enum TransparencyMode
		{
			Soft,
			Sharp
		}

		public enum DetailEmissionMode
		{
			Phase,
			AudioLink
		}

		protected Material target;
		protected MaterialEditor editor;
		protected Dictionary<string, MaterialProperty> props = new Dictionary<string, MaterialProperty>();

    	public int scssSettingsComplexityMode = (int)SettingsComplexityMode.Simple;

		protected bool usingLightramp = true; // Compatibility
		protected bool usingCrosstone = false;

		protected void CheckShaderType(Material material)
		{
			// Check material type
			const string crosstoneName = "Crosstone";
			usingCrosstone = material.shader.name.Contains(crosstoneName);

			const string lightrampName = "Lightramp";
			usingLightramp = material.shader.name.Contains(lightrampName);

			// Short circuit for old materials.
			const string hiddenName = "Old";
			if (material.shader.name.Contains(hiddenName)) UpgradeVariantCheck(material);
		}

		protected override void FindProperties(MaterialProperty[] matProps)
		{ 
			base.FindProperties(matProps);
			
			foreach (MaterialProperty prop in matProps)
			{
				props[prop.name] = FindProperty(prop.name, matProps, false);
			}
		}

		protected override void MaterialChanged(Material material)
		{
			InitialiseStyles();

			if (!Int32.TryParse(EditorUserSettings.GetConfigValue("scss_settings_complexity_mode"), out scssSettingsComplexityMode))
			{
				scssSettingsComplexityMode = (int)SettingsComplexityMode.Simple;
			}

			// Handle old materials
			UpgradeMatcaps(material);
			UpgradeVariantCheck(material);

			SetupMaterialWithAlbedo(material, 
				props["_MainTex"], 
				props["_AlbedoAlphaMode"]);

				
			MaterialProperty outlineProp;
			if (props.TryGetValue("_OutlineMode", out outlineProp))
			{
				SetupMaterialWithOutlineMode(material, (OutlineMode)outlineProp.floatValue);
			}

			MaterialProperty specProp;
			if (props.TryGetValue("_SpecularType", out specProp))
			{
				SetupMaterialWithSpecularType(material, (SpecularType)specProp.floatValue);
			}
			
			SetMaterialKeywords(material);

			base.MaterialChanged(material);
		}
		
		protected MaterialProperty Property(string i)
		{
			MaterialProperty prop;
			if (props.TryGetValue(i, out prop))
			{
				return prop;
			} 
			return null;
		}

		protected GUIContent Content(string i)
		{
			GUIContent style;
			if (!styles.TryGetValue(i, out style))
			{
				style = new GUIContent(i);
			}
			return style;
		}

		protected Rect DisabledLabel(GUIContent style)
		{
			EditorGUI.BeginDisabledGroup(true);
			Rect rect = EditorGUILayout.GetControlRect();
			EditorGUI.LabelField(rect, style);
			EditorGUI.EndDisabledGroup();
			return rect;
		}

		protected Rect TexturePropertySingleLine(string i)
		{
			MaterialProperty prop = Property(i);
			GUIContent style = Content(i);
			if (prop != null) 
			{
				return editor.TexturePropertySingleLine(style, prop);
			} else {
				return DisabledLabel(style);
			}
		}

		protected Rect TexturePropertySingleLine(string i, string i2)
		{
			GUIContent style = Content(i);
			MaterialProperty prop = Property(i);
			MaterialProperty prop2 = Property(i2);
			if (prop != null) 
			{
				return editor.TexturePropertySingleLine(style, prop, prop2);
			} else {
				return DisabledLabel(style);
			}
		}

		protected Rect TexturePropertySingleLine(string i, string i2, string i3)
		{
			GUIContent style = Content(i);
			MaterialProperty prop = Property(i);
			MaterialProperty prop2 = Property(i2);
			MaterialProperty prop3 = Property(i3);
			if (prop != null) 
			{
				return editor.TexturePropertySingleLine(style, prop, prop2, prop3);
			} else {
				return DisabledLabel(style);
			}
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
			GUIContent style = Content(i);
			MaterialProperty prop = Property(i);
			MaterialProperty prop2 = Property(i2);
			if (prop != null) 
			{
				return editor.TexturePropertyWithHDRColor(style, prop, prop2, false);
			} else {
				return DisabledLabel(style);
			}
		}

		protected bool ShaderProperty(string i)
		{
			MaterialProperty prop;
			GUIContent style;

			if (!styles.TryGetValue(i, out style))
			{
				style = new GUIContent(i);
			}

			if (props.TryGetValue(i, out prop))
			{
				editor.ShaderProperty(prop, style);
				return true;
			} else {
				DisabledLabel(style);
			}
			return false;
		}

		protected bool TogglePropertyHeader(string i, bool display = true)
		{
			if (display) return ShaderProperty(i);
			return false;
		}
		
        protected static void Vector2Property(MaterialProperty property, GUIContent name)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 vector2 = EditorGUILayout.Vector2Field(name,new Vector2(property.vectorValue.x, property.vectorValue.y),null);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(vector2.x, vector2.y, property.vectorValue.z, property.vectorValue.w);
        }
        protected static void Vector2PropertyZW(MaterialProperty property, GUIContent name)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 vector2 = EditorGUILayout.Vector2Field(name,new Vector2(property.vectorValue.z, property.vectorValue.w),null);
            if (EditorGUI.EndChangeCheck())
                property.vectorValue = new Vector4(property.vectorValue.x, property.vectorValue.y, vector2.x, vector2.y);
        }

        protected void DrawShaderPropertySameLine(string i) {
			MaterialProperty prop;

        	int HEADER_HEIGHT = 22; // Arktoon default
            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
            r.y -= HEADER_HEIGHT;
            r.height = MaterialEditor.GetDefaultPropertyHeight(props[i]);

			if (props.TryGetValue(i, out prop))
			{
				editor.ShaderProperty(r, prop, " ");
			} 
        }

		protected GUIStyle scmStyle;
		protected GUIStyle sectionHeader;
		protected GUIStyle sectionHeaderBox;

		protected void InitialiseStyles()
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

		protected Rect DrawSectionHeaderArea(GUIContent content)
		{
            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
				r.x -= 2.0f;
				r.y += 2.0f;
				r.height = 18.0f;
				r.width -= 0.0f;
			GUI.Box(r, EditorGUIUtility.IconContent("d_FilterByType"), sectionHeaderBox);
			EditorGUILayout.LabelField(content, sectionHeader);
			return r;
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] matProps)
		{ 
			this.target = materialEditor.target as Material;
			this.editor = materialEditor;
			Material material = this.target;

			CheckShaderType(material);
			
			ShaderBakeHeader();
			
			bool isBaked = false;
			{
				MaterialProperty bakedSettings;
				props.TryGetValue("__Baked", out bakedSettings);
				isBaked = (bakedSettings != null && bakedSettings.floatValue == 1);
			}

        	using (new EditorGUI.DisabledScope(isBaked == true))
			{
				base.OnGUI(materialEditor, matProps);
			}
			SettingsComplexityArea();
			
			switch ((SettingsComplexityMode)scssSettingsComplexityMode)
			{
				case SettingsComplexityMode.Simple:
					using (new EditorGUI.DisabledScope(isBaked == true))
					{
					MainOptions();
					ShadingOptions();
					DrawSectionHeaderArea(Content("s_renderingOptions"));
					EmissionOptions();
					OutlineOptions();
            		}
					ManualButtonArea();
					break;
				case SettingsComplexityMode.Normal:
					using (new EditorGUI.DisabledScope(isBaked == true))
					{
					MainOptions();
					ShadingOptions();
					RenderingOptions();
					OutlineOptions();
					EmissionOptions();
            		}
					RuntimeLightOptions();
					InventoryOptions(isBaked);
					ManualButtonArea();
					using (new EditorGUI.DisabledScope(isBaked == true))
					{
					AdvancedOptions();
					}
					break;
				default:
				case SettingsComplexityMode.Complex:
					using (new EditorGUI.DisabledScope(isBaked == true))
					{
					MainOptions();
					ShadingOptions();
					RenderingOptions();
					OutlineOptions();
					DetailOptions();
					EmissionOptions();
					MiscOptions();
            		}
					RuntimeLightOptions();
					InventoryOptions(isBaked);
					ManualButtonArea();
					using (new EditorGUI.DisabledScope(isBaked == true))
					{
					AdvancedOptions();
					}
					break;
			}
			
			FooterOptions();
		}

		protected string[] SettingsComplexityModeOptions = new string[]
		{
			"Complex", "Normal", "Simple"
		};

		protected void SettingsComplexityArea()
		{
			SettingsComplexityModeOptions[0] = Content("s_fullComplexity").text;
			SettingsComplexityModeOptions[1] = Content("s_normalComplexity").text;
			SettingsComplexityModeOptions[2] = Content("s_simpleComplexity").text;
			EditorGUILayout.Space();

			if (WithChangeCheck(() =>
			{
				scssSettingsComplexityMode = EditorGUILayout.Popup(scssSettingsComplexityMode, SettingsComplexityModeOptions, scmStyle);
			})) 
			{
				EditorUserSettings.SetConfigValue("scss_settings_complexity_mode", scssSettingsComplexityMode.ToString());
			}
		}

		protected void ShaderBakeHeader()
		{
            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
				r.x -= 12.0f;
				r.y -= 8.0f;
				r.height = 18.0f;
				r.width += 4.0f;
			float maxWidth = 128.0f;
			Rect r2 = r;
				r2.x = r.width - maxWidth + 14.0f;
				r2.width = maxWidth;
			GUI.Box(r, "", EditorStyles.toolbar);

			GUIContent s_bakeButton;
			MaterialProperty shaderOptimizer;
			// Create the GUIContent for the button so it can be rendered.
			if (!props.TryGetValue("__Baked", out shaderOptimizer))
			{
				s_bakeButton = new GUIContent ("s_bakeButton");
			}
			else
			{
				// Determine whether we're baking or unbaking materials. 
				if (shaderOptimizer.floatValue == 1) 
				{
					if (!styles.TryGetValue("s_bakeButtonRevert", out s_bakeButton)) 
						s_bakeButton = new GUIContent ("s_bakeButtonRevert");
				} 
				else
				{
					if (editor.targets.Length == 1)
					{
						if (!styles.TryGetValue("s_bakeButton", out s_bakeButton)) 
							s_bakeButton = new GUIContent ("s_bakeButton");
					} 
					else 
					{
						if (!styles.TryGetValue("s_bakeButtonPlural", out s_bakeButton)) 
							s_bakeButton = new GUIContent ("s_bakeButtonPlural");
						s_bakeButton = new GUIContent(s_bakeButton);
						s_bakeButton.text = String.Format(s_bakeButton.text, "" + editor.targets.Length.ToString());
					}
				}
			}
			// Draw the button. Because of Unity shenanigans, if we don't always draw the button, 
			// the layout will explode on the first update of the inspector.
			if (GUI.Button(r2, s_bakeButton, EditorStyles.miniButtonMid))
			{
				// If it's a mixed value, then only allow baking. It shouldn't be a mixed value,
				// but a material might think it's baked when it isn't, which needs to be handled anyway.
				if (shaderOptimizer.hasMixedValue)
				{
					foreach (Material m in editor.targets)
					{
						m.SetFloat(shaderOptimizer.name, 1);
						MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { m });
						if (!ShaderOptimizer.Lock(m, props)) // Error locking shader, revert property
							m.SetFloat(shaderOptimizer.name, 0);
					}	

				}
				else
				{
					shaderOptimizer.floatValue = shaderOptimizer.floatValue == 1 ? 0 : 1;
					if (shaderOptimizer.floatValue == 1)
					{
						foreach (Material m in editor.targets)
						{
							MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { m });
							if (!ShaderOptimizer.Lock(m, props))
								m.SetFloat(shaderOptimizer.name, 0);
						}
					}
					else
					{
						foreach (Material m in editor.targets)
						{
							if (!ShaderOptimizer.Unlock(m))
								m.SetFloat(shaderOptimizer.name, 1);
							MaterialChanged(m);
						}
					}
				}
			}
			EditorGUILayout.LabelField("", EditorStyles.label); // Spacing only
		}
		
		protected void MainOptions()
		{ 
			EditorGUILayout.Space();
		
			DrawSectionHeaderArea(Content("s_mainOptions"));

			EditorGUILayout.Space();
			TexturePropertySingleLine("_MainTex", "_Color");

			TexturePropertySingleLine("_BumpMap", "_BumpScale");

			TexturePropertySingleLine("_ColorMask");

			// For Standard compatibility, but not sure what the purpose is
			if (WithChangeCheck(() => 
			{
				editor.TextureScaleOffsetProperty(props["_MainTex"]);
			}))
			{
				props["_EmissionMap"].textureScaleAndOffset = props["_MainTex"].textureScaleAndOffset;		
			}

			if ((AlbedoAlphaMode)props["_AlbedoAlphaMode"].floatValue == AlbedoAlphaMode.ClippingMask)
			{
				EditorGUILayout.Space();
				TexturePropertySingleLine("_ClippingMask", "_Tweak_Transparency");
				editor.TextureScaleOffsetProperty(props["_ClippingMask"]);
			}
			EditorGUILayout.Space();

			if ((RenderingMode)props[BaseStyles.renderingModeName].floatValue > 0)
			{
				foreach (Material mat in WithMaterialPropertyDropdown(props["_AlphaSharp"], Enum.GetNames(typeof(TransparencyMode)), editor))
				{
					SetupMaterialWithTransparencyMode(mat, (TransparencyMode)props["_AlphaSharp"].floatValue);
				}
				ShaderProperty("_Cutoff");
			}
		}

		protected void ShadingOptions()
		{
			EditorGUILayout.Space();
			DrawSectionHeaderArea(Content("s_shadingOptions"));
			if (usingLightramp) LightrampOptions();
			if (usingCrosstone) CrosstoneOptions();
		}

		protected void RenderingOptions()
		{ 
			EditorGUILayout.Space();
			DrawSectionHeaderArea(Content("s_renderingOptions"));

			SpecularOptions();
			RimlightOptions();
			MatcapOptions();
		}

		protected void DetailOptions()
		{
			EditorGUILayout.Space();
			DrawSectionHeaderArea(Content("s_detailOptions"));
			SubsurfaceOptions();
			DetailMapOptions();
		}

		protected void EmissionOptions()
		{
			EditorGUILayout.Space();

			MaterialProperty emissionMapProp;
			if (props.TryGetValue("_EmissionMap", out emissionMapProp)) 
			{
				bool hadEmissionTexture = emissionMapProp.textureValue != null;
				TexturePropertyWithHDRColor("_EmissionMap", "_EmissionColor");
				// If texture was assigned and color was black set color to white
				float brightness = props["_EmissionColor"].colorValue.maxColorComponent;
				if (emissionMapProp.textureValue != null && !hadEmissionTexture && brightness <= 0f)
					props["_EmissionColor"].colorValue = Color.white;
				editor.TextureScaleOffsetProperty(props["_EmissionMap"]);
				EditorGUILayout.Space();
			}

			if (ShaderProperty("_UseAdvancedEmission") && PropertyEnabled(props["_UseAdvancedEmission"]))
			{
				target.EnableKeyword("_EMISSION");
				TexturePropertySingleLine("_DetailEmissionMap");
				editor.TextureScaleOffsetProperty(props["_DetailEmissionMap"]);
				ShaderProperty("_DetailEmissionUVSec");
				EditorGUI.indentLevel ++;
				
				WithMaterialPropertyDropdown(props["_EmissionDetailType"], Enum.GetNames(typeof(DetailEmissionMode)), editor);
				MaterialProperty deProp;
				if (props.TryGetValue("_EmissionDetailType", out deProp))
				{
					switch ((DetailEmissionMode)deProp.floatValue)
					{
						case DetailEmissionMode.Phase:
						//ShaderProperty("_EmissionDetailParams");
						Vector2Property(props["_EmissionDetailParams"], Content("s_EmissionDetailScroll"));
						Vector2PropertyZW(props["_EmissionDetailParams"], Content("s_EmissionDetailPhase"));
						break;
						case DetailEmissionMode.AudioLink:
						// AudioLink
						ShaderProperty("_alColorR");
						ShaderProperty("_alColorG");
						ShaderProperty("_alColorB");
						ShaderProperty("_alColorA");
						ShaderProperty("_alBandR");
						ShaderProperty("_alBandG");
						ShaderProperty("_alBandB");
						ShaderProperty("_alBandA");
						ShaderProperty("_alModeR");
						ShaderProperty("_alModeG");
						ShaderProperty("_alModeB");
						ShaderProperty("_alModeA");
						ShaderProperty("_alTimeRangeR");
						ShaderProperty("_alTimeRangeG");
						ShaderProperty("_alTimeRangeB");
						ShaderProperty("_alTimeRangeA");
						ShaderProperty("_alUseFallback");
						ShaderProperty("_alFallbackBPM");
						ShaderProperty("_UseEmissiveLightSense");
						ShaderProperty("_EmissiveLightSenseStart");
						ShaderProperty("_EmissiveLightSenseEnd");
						break;
						default:
						break;
					}
				}
            	EditorGUI.indentLevel --;
			} else {
				target.DisableKeyword("_EMISSION");
			}
			EditorGUILayout.Space();
			ShaderProperty("_CustomFresnelColor");
			// For some reason, this property doesn't have spacing after it
			EditorGUILayout.Space();
		}

		protected void MiscOptions()
		{
			EditorGUILayout.Space();
			DrawSectionHeaderArea(Content("s_miscOptions"));
			EditorGUILayout.Space();
			ShaderProperty("_PixelSampleMode");
			AnimationOptions();
			VanishingOptions();
		}

        protected void LightrampOptions()
        { 
			EditorGUILayout.Space();
			
			foreach (Material mat in WithMaterialPropertyDropdown(props["_LightRampType"], Enum.GetNames(typeof(LightRampType)), editor))
			{
				SetupMaterialWithLightRampType(mat, (LightRampType)props["_LightRampType"].floatValue);
			}
			
            if ((LightRampType)props["_LightRampType"].floatValue != LightRampType.None) 
            {
                WithGroupHorizontal(() => 
				{
					TexturePropertySingleLine("_Ramp");
					if (GUILayout.Button(Content("s_gradientEditorButton"), "button"))
					{
						XSGradientEditor.callGradientEditor(target);
					}
				});
            }
            ShaderProperty("_ShadowLift");
            ShaderProperty("_IndirectLightingBoost");
            
            EditorGUILayout.Space();

            TexturePropertySingleLine("_ShadowMask", "_ShadowMaskColor");

			foreach (Material mat in WithMaterialPropertyDropdown(props["_ShadowMaskType"], Enum.GetNames(typeof(ShadowMaskType)), editor))
			{
				SetupMaterialWithShadowMaskType(mat, (ShadowMaskType)props["_ShadowMaskType"].floatValue);
			}

            ShaderProperty("_Shadow");
        }
		
		protected void CrosstoneOptions()
		{ 
            EditorGUILayout.Space();
			WithGroupHorizontal(() => {
				TextureColorPropertyWithColorReset("_1st_ShadeMap", "_1st_ShadeColor");
				WithMaterialPropertyDropdownNoLabel(props["_CrosstoneToneSeparation"], Enum.GetNames(typeof(ToneSeparationType)), editor);
			});
			ShaderProperty("_1st_ShadeColor_Step");
			ShaderProperty("_1st_ShadeColor_Feather");
            EditorGUILayout.Space();
			
			WithGroupHorizontal(() => {
				TextureColorPropertyWithColorReset("_2nd_ShadeMap", "_2nd_ShadeColor");
				WithMaterialPropertyDropdownNoLabel(props["_Crosstone2ndSeparation"], Enum.GetNames(typeof(ToneSeparationType)), editor);
			});
			ShaderProperty("_2nd_ShadeColor_Step");
			ShaderProperty("_2nd_ShadeColor_Feather");
            EditorGUILayout.Space();

			TexturePropertySingleLine("_ShadingGradeMap", "_Tweak_ShadingGradeMapLevel");
		}

		protected void SpecularOptions()
		{	
            EditorGUILayout.Space();
			MaterialProperty specProp;
			if (props.TryGetValue("_SpecularType", out specProp))
			{
				foreach (Material mat in WithMaterialPropertyDropdown(specProp, Enum.GetNames(typeof(SpecularType)), editor))
				{
					SetupMaterialWithSpecularType(mat, (SpecularType)specProp.floatValue);
				}
				TogglePropertyHeader("_SpecularType", false);

				switch ((SpecularType)specProp.floatValue)
				{
					case SpecularType.Standard:
					case SpecularType.Cloth:
					TextureColorPropertyWithColorReset("_SpecGlossMap", "_SpecColor");
					ShaderProperty("_Smoothness");
					ShaderProperty("_UseMetallic");
					ShaderProperty("_UseEnergyConservation");
					break;
					case SpecularType.Cel:
					TextureColorPropertyWithColorReset("_SpecGlossMap", "_SpecColor");
					ShaderProperty("_Smoothness");
					ShaderProperty("_CelSpecularSoftness");
					ShaderProperty("_CelSpecularSteps");
					ShaderProperty("_UseMetallic");
					ShaderProperty("_UseEnergyConservation");
					break;
					case SpecularType.Anisotropic:
					TextureColorPropertyWithColorReset("_SpecGlossMap", "_SpecColor");
					ShaderProperty("_Smoothness");
					ShaderProperty("_Anisotropy");
					ShaderProperty("_UseMetallic");
					ShaderProperty("_UseEnergyConservation");
					break;
					case SpecularType.CelStrand:
					TextureColorPropertyWithColorReset("_SpecGlossMap", "_SpecColor");
					ShaderProperty("_Smoothness");
					ShaderProperty("_CelSpecularSoftness");
					ShaderProperty("_CelSpecularSteps");
					ShaderProperty("_Anisotropy");
					ShaderProperty("_UseMetallic");
					ShaderProperty("_UseEnergyConservation");
					break;
					case SpecularType.Disable:
					default:
					break;
				}	
			}
		}

		protected void RimlightOptions()
		{	
            EditorGUILayout.Space();
			MaterialProperty rimProp;
			if (props.TryGetValue("_UseFresnel", out rimProp))
			{
				TogglePropertyHeader("_UseFresnel");
				bool isTintable = 
					(AmbientFresnelType)rimProp.floatValue == AmbientFresnelType.Lit
					|| (AmbientFresnelType)rimProp.floatValue == AmbientFresnelType.Ambient;

				if (PropertyEnabled(rimProp))
				{
					ShaderProperty("_FresnelWidth");
					ShaderProperty("_FresnelStrength");
					if (isTintable) ShaderProperty("_FresnelTint");	

					ShaderProperty("_UseFresnelLightMask");
					if (PropertyEnabled(props["_UseFresnelLightMask"]))
					{
						ShaderProperty("_FresnelLightMask");
						if (isTintable) ShaderProperty("_FresnelTintInv");
						ShaderProperty("_FresnelWidthInv");
						ShaderProperty("_FresnelStrengthInv");
					}
				}
			}
		}

		private void DrawMatcapField(string texture, string blend, string tint, string strength)
		{
			WithGroupHorizontal(() => {
				TextureColorPropertyWithColorReset(texture, tint);
				WithMaterialPropertyDropdownNoLabel(props[blend], Enum.GetNames(typeof(MatcapBlendModes)), editor);
			});
			EditorGUI.indentLevel+=2;
			ShaderProperty(strength);
			EditorGUI.indentLevel-=2;
		}

		protected void MatcapOptions()
		{ 
			EditorGUILayout.Space();
			MaterialProperty matcapProp;
			if (props.TryGetValue("_UseMatcap", out matcapProp))
			{
				var mMode = (MatcapType)matcapProp.floatValue;
				if (WithChangeCheck(() => 
				{
					mMode = (MatcapType)EditorGUILayout.Popup(Content("_UseMatcap"), 
					(int)mMode, Enum.GetNames(typeof(MatcapType)));
				})) {
					editor.RegisterPropertyChangeUndo(Content("_UseMatcap").text);
					matcapProp.floatValue = (float)mMode;
				}
				TogglePropertyHeader("_UseMatcap", false);

				if (PropertyEnabled(matcapProp))
				{
					TexturePropertySingleLine("_MatcapMask");
					DrawMatcapField("_Matcap1", "_Matcap1Blend", "_Matcap1Tint", "_Matcap1Strength");
					DrawMatcapField("_Matcap2", "_Matcap2Blend", "_Matcap2Tint", "_Matcap2Strength");
					DrawMatcapField("_Matcap3", "_Matcap3Blend", "_Matcap3Tint", "_Matcap3Strength");
					DrawMatcapField("_Matcap4", "_Matcap4Blend", "_Matcap4Tint", "_Matcap4Strength");
				}
			}
		}

		protected void SubsurfaceOptions()
		{ 
            EditorGUILayout.Space();
			if (TogglePropertyHeader("_UseSubsurfaceScattering"))
			{
				if (PropertyEnabled(props["_UseSubsurfaceScattering"]))
				{
					TexturePropertySingleLine("_ThicknessMap");
					ShaderProperty("_ThicknessMapPower");
					ShaderProperty("_ThicknessMapInvert");
					ShaderProperty("_SSSCol");
					ShaderProperty("_SSSIntensity");
					ShaderProperty("_SSSPow");
					ShaderProperty("_SSSDist");
					ShaderProperty("_SSSAmbient");
				}
			}
		}

		protected void DetailMapOptions()
		{	  
            EditorGUILayout.Space();
			if (TogglePropertyHeader("_UseDetailMaps")){
				if (PropertyEnabled(props["_UseDetailMaps"])) 
				{
					target.EnableKeyword("_DETAIL_MULX2");
					TexturePropertySingleLine("_DetailAlbedoMap", "_DetailAlbedoMapScale");
					TexturePropertySingleLine("_DetailNormalMap", "_DetailNormalMapScale");
					TexturePropertySingleLine("_SpecularDetailMask", "_SpecularDetailStrength");
					
					editor.TextureScaleOffsetProperty(props["_DetailAlbedoMap"]);
					ShaderProperty("_UVSec");
				} else {
					target.DisableKeyword("_DETAIL_MULX2");
				}
			}
		}

		protected void AnimationOptions()
		{
            EditorGUILayout.Space();
			if (TogglePropertyHeader("_UseAnimation") && PropertyEnabled(props["_UseAnimation"]))
			{
				ShaderProperty("_AnimationSpeed");
				ShaderProperty("_TotalFrames");
				ShaderProperty("_FrameNumber");
				ShaderProperty("_Columns");
				ShaderProperty("_Rows");
			}

		}

		protected void VanishingOptions()
		{ 
            EditorGUILayout.Space();
			if (TogglePropertyHeader("_UseVanishing") && PropertyEnabled(props["_UseVanishing"]))
			{
				ShaderProperty("_VanishingStart");
				ShaderProperty("_VanishingEnd");
			}
		}

		protected void OutlineOptions()
		{ 
			EditorGUILayout.Space();
			
			MaterialProperty outlineProp;
			if (props.TryGetValue("_OutlineMode", out outlineProp))
			{
				foreach (Material mat in WithMaterialPropertyDropdown(outlineProp, Enum.GetNames(typeof(OutlineMode)), editor))
				{
					SetupMaterialWithOutlineMode(mat, (OutlineMode)outlineProp.floatValue);
				}
				TogglePropertyHeader("_OutlineMode", false);

				switch ((OutlineMode)outlineProp.floatValue)
				{
					case OutlineMode.Tinted:
					case OutlineMode.Colored:
					TexturePropertySingleLine("_OutlineMask");
					ShaderProperty("_outline_color");
					ShaderProperty("_outline_width");
					ShaderProperty("_OutlineZPush");
					break;
					case OutlineMode.None:
					default:
					break;
				}	  
			}
		}

		protected void ManualButtonArea()
		{
			EditorGUILayout.Space();
			
			DrawSectionHeaderArea(Content("Resources"));

            Rect r = EditorGUILayout.GetControlRect(true,0,EditorStyles.layerMaskField);
				r.x -= 2.0f;
				r.y += 2.0f;
				r.height = 18.0f;
			Rect r2 = r;
				r2.width = r.width / 2.0f;
			//GUI.Box(r, EditorGUIUtility.IconContent("Toolbar"), EditorStyles.toolbar);
			if (GUI.Button(r2, Content("s_manualButton"), EditorStyles.miniButtonLeft)) Application.OpenURL("https://gitlab.com/s-ilent/SCSS/wikis/Manual/Setting-Overview");
				r2.x += r2.width;
			if (GUI.Button(r2, Content("s_socialButton"), EditorStyles.miniButtonRight)) Application.OpenURL("https://discord.gg/uHJx4g629K");
			EditorGUILayout.LabelField("", EditorStyles.label);
		}

		protected void RuntimeLightOptions()
		{
			EditorGUILayout.Space();
			ShaderProperty("_LightMultiplyAnimated");
			ShaderProperty("_LightClampAnimated");
		}

		protected void InventoryOptions(bool isBaked)
		{
			EditorGUILayout.Space();
			DrawSectionHeaderArea(Content("s_inventoryOptions"));
			EditorGUILayout.Space();

			MaterialProperty invProp;
			bool[] enabledItems = new bool[16];
			float toggleOptionWidth = (EditorGUIUtility.currentViewWidth / 5.0f); // blursed

			if (props.TryGetValue("_UseInventory", out invProp))
			{
				using (new EditorGUI.DisabledScope(isBaked == true))
				{
					TogglePropertyHeader("_UseInventory");
					if (PropertyEnabled(invProp)) ShaderProperty("_InventoryStride");
				}
				if (PropertyEnabled(invProp))
				{
					for (int i = 1; i <= 16; i++)
					{
						enabledItems[i-1] = Property(String.Format("_InventoryItem{0:00}Animated", i)).floatValue == 1;
					}
					EditorGUI.BeginChangeCheck();
					for (int i = 0; i < (16/4); i++)
					{	EditorGUILayout.BeginHorizontal("Box");
						enabledItems[i*4+0] = EditorGUILayout.ToggleLeft(
							(i*4+1).ToString(), enabledItems[i*4+0], GUILayout.Width(toggleOptionWidth));
						enabledItems[i*4+1] = EditorGUILayout.ToggleLeft(
							(i*4+2).ToString(), enabledItems[i*4+1], GUILayout.Width(toggleOptionWidth));
						enabledItems[i*4+2] = EditorGUILayout.ToggleLeft(
							(i*4+3).ToString(), enabledItems[i*4+2], GUILayout.Width(toggleOptionWidth));
						enabledItems[i*4+3] = EditorGUILayout.ToggleLeft(
							(i*4+4).ToString(), enabledItems[i*4+3], GUILayout.Width(toggleOptionWidth));
						EditorGUILayout.EndHorizontal();
					};
					if (EditorGUI.EndChangeCheck())
					{
					for (int i = 1; i <= 16; i++)
						{
							Property(String.Format("_InventoryItem{0:00}Animated", i)).floatValue = enabledItems[i-1] ? 1 : 0;
						}
					};
				}
			}
		}

		protected void AdvancedOptions()
		{
			EditorGUILayout.Space();

			DrawSectionHeaderArea(Content("s_advancedOptions"));

			EditorGUILayout.Space();

			foreach (Material mat in WithMaterialPropertyDropdown(props["_VertexColorType"], Enum.GetNames(typeof(VertexColorType)), editor))
			{
				SetupMaterialWithVertexColorType(mat, (VertexColorType)props["_VertexColorType"].floatValue);
			}

			foreach (Material mat in WithMaterialPropertyDropdown(props["_AlbedoAlphaMode"], CommonStyles.albedoAlphaModeNames, editor))
			{
				SetupMaterialWithAlbedo(mat, props["_MainTex"], props["_AlbedoAlphaMode"]);
			}

			foreach (Material mat in WithMaterialPropertyDropdown(props["_LightingCalculationType"], Enum.GetNames(typeof(LightingCalculationType)), editor))
			{
				SetupMaterialWithLightingCalculationType(mat, (LightingCalculationType)props["_LightingCalculationType"].floatValue);
			}

			ShaderProperty("_IndirectShadingType");

			EditorGUILayout.Space();

			ShaderProperty("_DiffuseGeomShadowFactor");
			ShaderProperty("_LightWrappingCompensationFactor");

			ShaderProperty("_LightSkew");

			MaterialProperty specProp;
			if (props.TryGetValue("_SpecularType", out specProp) && specProp.floatValue >= 1.0f) 
			{
				if (WithChangeCheck(() => 
				{
					ShaderProperty("_SpecularHighlights");
					ShaderProperty("_GlossyReflections");
				})) {
					MaterialChanged(target);
				}
			};

			StencilOptions(editor, target);
		}

		protected void FooterOptions()
		{
			EditorGUILayout.Space();

			if (WithChangeCheck(() => 
			{
				editor.ShaderProperty(renderQueueOverride, BaseStyles.renderQueueOverride);
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

		protected static void SetupMaterialWithTransparencyMode(Material material, TransparencyMode shadowMaskType)
		{
			switch ((TransparencyMode)material.GetFloat("_AlphaSharp"))
			{
				case TransparencyMode.Sharp:
				material.SetFloat("_AlphaSharp", 1);
				break;
				default:
				case TransparencyMode.Soft:
				material.SetFloat("_AlphaSharp", 0);
				break;
			}
		}

		protected static void SetupMaterialWithShadowMaskType(Material material, ShadowMaskType shadowMaskType)
		{
			switch ((ShadowMaskType)material.GetFloat("_ShadowMaskType"))
			{
				case ShadowMaskType.Occlusion:
				material.SetFloat("_ShadowMaskType", 0);
				break;
				case ShadowMaskType.Tone:
				material.SetFloat("_ShadowMaskType", 1);
				break;
				case ShadowMaskType.Auto:
				material.SetFloat("_ShadowMaskType", 2);
				break;
				default:
				break;
			}
		}

		protected static void SetupMaterialWithLightRampType(Material material, LightRampType lightRampType)
		{
			switch ((LightRampType)material.GetFloat("_LightRampType"))
			{
				case LightRampType.Horizontal:
				material.SetFloat("_LightRampType", 0);
				break;
				case LightRampType.Vertical:
				material.SetFloat("_LightRampType", 1);
				break;
				case LightRampType.None:
				material.SetFloat("_LightRampType", 2);
				break;
				default:
				break;
			}
		}

protected float? GetSerializedMaterialFloat(Material material, string propName)
{
	float? floatVal = new SerializedObject(material).FindProperty("m_SavedProperties.m_Floats." + propName).floatValue;
	return floatVal;
}

protected Vector4? GetSerializedMaterialVector4(Material material, string propName)
{
	Vector4? colorVal = new SerializedObject(material).FindProperty("m_SavedProperties.m_Colors." + propName).colorValue;
	return colorVal;
}

		protected void UpgradeMatcaps(Material material)
		{
		// Check if the new properties exist.
		// NOTE: This is written with the current Unity behaviour in mind.
		// - GetFloat returns NULL for properties not in the CURRENT shader.
		// - GetTexture returns textures for properties not in the current shader.
		// If GetFloat gets changed, intensity transfer will work.
		// If GetTexture gets changed, the whole thing will break. 

			bool oldMatcaps = 
			(material.GetFloat("_UseMatcap") == 1.0) &&
			(material.GetFloat("_Matcap1Strength") == 1.0 && material.GetFloat("_Matcap2Strength") == 1.0 &&
				material.GetFloat("_Matcap3Strength") == 1.0 && material.GetFloat("_Matcap4Strength") == 1.0) &&
			(props["_Matcap1"].textureValue == null && props["_Matcap2"].textureValue == null &&
				props["_Matcap3"].textureValue == null && props["_Matcap4"].textureValue == null) &&
		 GetTextureProperty(material, "_AdditiveMatcap"); // Only exists in old materials.
		 if (oldMatcaps)
		 {
		// GetFloat is bugged but GetTexture is not, so we use GetFloatProperty so we can handle null.
		 	float? additiveStrength = GetFloatProperty(material, "_AdditiveMatcapStrength");
		 	float? multiplyStrength = GetFloatProperty(material, "_MultiplyMatcapStrength");
		 	float? medianStrength = GetFloatProperty(material, "_MidBlendMatcapStrength");
		 	Texture additiveMatcap = GetTextureProperty(material, "_AdditiveMatcap");
		 	Texture multiplyMatcap = GetTextureProperty(material, "_MultiplyMatcap");
		 	Texture medianMatcap = GetTextureProperty(material, "_MidBlendMatcap");

		// Mask layout is RGBA
		 	if (additiveMatcap)
		 	{
		 		props["_Matcap2"].textureValue = additiveMatcap;
		 		props["_Matcap2Blend"].floatValue = (float)MatcapBlendModes.Additive;
		 		props["_Matcap2Strength"].floatValue = additiveStrength ?? 1;
		 	}
		 	if (multiplyMatcap) 
		 	{
		 		props["_Matcap4"].textureValue = multiplyMatcap;
		 		props["_Matcap4Blend"].floatValue = (float)MatcapBlendModes.Multiply;
		 		props["_Matcap4Strength"].floatValue = multiplyStrength ?? 0; 
			// Multiply at 1.0 is usually wrong. This also prevents oldMatcaps from being true.
		 	}
		 	if (medianMatcap) 
		 	{
		 		props["_Matcap3"].textureValue = medianMatcap;
		 		props["_Matcap3Blend"].floatValue = (float)MatcapBlendModes.Median;
		 		props["_Matcap3Strength"].floatValue = medianStrength ?? 1;
		 	}
		 }
		}
		// Taken from Standard. Only Standard keywords are set here!
		protected static void SetMaterialKeywords(Material material)
		{
				// Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
				// (MaterialProperty value might come from renderer material property block)
			SetKeyword(material, "_NORMALMAP", 
				GetTextureProperty(material, "_BumpMap") && GetTextureProperty(material, "_DetailNormalMap"));
				/*
				SetKeyword(material, "_SPECGLOSSMAP", GetTextureProperty(material, "_SpecGlossMap"));
				SetKeyword(material, "_PARALLAXMAP", GetTextureProperty(material, "_ParallaxMap"));
				SetKeyword(material, "_DETAIL_MULX2", GetTextureProperty(material, "_DetailAlbedoMap") 
					&& GetTextureProperty(material, "_DetailNormalMap")
					&& GetTextureProperty(material, "_DetailEmissionMap")
					&& GetTextureProperty(material, "_SpecularDetailMask"));
				*/
/*
				// A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
				// or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
				// The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
					MaterialEditor.FixupEmissiveFlag(material);
					bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
					SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
*/
		}


        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
			// Use nullable types for Color and float, because Texture is nullable.
			Dictionary<string, Color?> tColor = new Dictionary<string, Color?>();
			Dictionary<string, float?> tFloat = new Dictionary<string, float?>();
			Dictionary<string, Texture> tTexture = new Dictionary<string, Texture>();

            // Cache old shader properties with potentially different names than the new shader.
            Vector4? textureScaleOffset = null;
            float? cullMode = GetFloatProperty(material, "_Cull");
			
			// Register properties that already exist but may be overridden.
			string[] colorProps = {
				"_EmissionColor",
				"_FresnelTint",
				"_FresnelTintInv",
				"_Matcap1Tint",
				"_outline_color",
				"_SpecColor"
			};

			string[] floatProps = {
				"_1st_ShadeColor_Feather",
				"_1st_ShadeColor_Step",
				"_2nd_ShadeColor_Feather",
				"_2nd_ShadeColor_Step",
				"_BumpScale",
				"_CelSpecularSoftness",
				"_Crosstone2ndSeparation",
				"_CrosstoneToneSeparation",
				"_FresnelLightMask",
				"_FresnelStrength",
				"_FresnelStrengthInv",
				"_FresnelWidth",
				"_FresnelWidthInv",
				"_Matcap1Blend",
				"_Matcap1Strength",
				"_outline_width",
				"_OutlineMode",
				"_Smoothness",
				"_SpecularType",
				"_UseFresnel",
				"_UseFresnelLightMask",
				"_UseMatcap"
			};

			string[] textureProps = {
				"_1st_ShadeMap",
				"_2nd_ShadeMap",
				"_BumpMap",
				"_EmissionMap",
				"_Matcap1",
				"_MatcapMask",
				"_OutlineMask",
				"_SpecGlossMap",
				"_SpecularDetailMask"
			};

			foreach (string p in colorProps) { tColor[p] = GetColorProperty(material, p); };
			foreach (string p in floatProps) { tFloat[p] = GetFloatProperty(material, p); };
			foreach (string p in textureProps) { tTexture[p] = GetTextureProperty(material, p); };

            int? stencilReference = GetIntProperty(material, "_Stencil");
            int? stencilComparison = GetIntProperty(material, "_StencilComp");
            int? stencilOperation = GetIntProperty(material, "_StencilOp");
            int? stencilFail = GetIntProperty(material, "_StencilFail");

            if (oldShader)
            {
				if (oldShader.name.Contains("Silent's Cel Shading"))
				{
					// Handle the case where someone swaps the outline mode by changing from 
					// the (Outline) to the no outline shader.
					if (oldShader.name.Contains("Outline") && !newShader.name.Contains("Outline"))
					{
						tFloat["_OutlineMode"] = 0.0f;
					}
					if (!oldShader.name.Contains("Outline") && newShader.name.Contains("Outline"))
					{
						tFloat["_OutlineMode"] = 1.0f;
					}
					// Handle transferring from really old versions.
					if (oldShader.name.Contains(TransparentCutoutShadersPath))
					{
						tFloat[BaseStyles.renderingModeName] = (float)RenderingMode.Cutout;
						tFloat[BaseStyles.customRenderingModeName] = (float)CustomRenderingMode.Cutout;
					}
					else if (oldShader.name.Contains(TransparentShadersPath))
					{
						tFloat[BaseStyles.renderingModeName] = (float)RenderingMode.Fade;
						tFloat[BaseStyles.customRenderingModeName] = (float)CustomRenderingMode.Fade;
					}
				}
            	if (oldShader.name.Contains("UnityChanToonShader"))
                {
					// Build translation table.
                    tTexture["_BumpMap"] = GetTextureProperty(material, "_NormalMap");
                    // _Tweak_ShadingGradeMapLevel is named the same.

					if (GetFloatProperty(material, "_Inverse_Clipping") == 1) Debug.Log("Note: Inverse clipping currently not supported.");
					if (GetTextureProperty(material, "_ClippingMask")) tFloat["_AlbedoAlphaMode"] = (float)AlbedoAlphaMode.ClippingMask;
                    tFloat["_Tweak_Transparency"] = GetFloatProperty(material, "_Tweak_transparency");
                    tFloat["_Cutoff"] = 1.0f - GetFloatProperty(material, "_Clipping_Level") ?? 0;
					
                    // Tone seperation is based on whether BaseAs1st is set.
                    // 2nd seperation is based on whether 1stAs2nd is set.
                    tFloat["_CrosstoneToneSeparation"] = 1.0f - GetFloatProperty(material, "_Use_BaseAs1st") ?? 0;
                    tFloat["_Crosstone2ndSeparation"] = 1.0f - GetFloatProperty(material, "_Use_1stAs2nd") ?? 0;
					
                    if (oldShader.name.Contains("DoubleShadeWithFeather"))
                    {
                        tFloat["_1st_ShadeColor_Step"]    = GetFloatProperty(material, "_BaseColor_Step");
                        tFloat["_1st_ShadeColor_Feather"] = GetFloatProperty(material, "_BaseShade_Feather");
                        tFloat["_2nd_ShadeColor_Step"]    = GetFloatProperty(material, "_ShadeColor_Step");
                        tFloat["_2nd_ShadeColor_Feather"] = GetFloatProperty(material, "_1st2nd_Shades_Feather");
                    }

					// Emission properties are not fully supported.
					tTexture["_EmissionMap"] = GetTextureProperty(material, "_Emissive_Tex");
					tColor["_EmissionColor"] = GetColorProperty(material, "_Emissive_Color");

                    // HighColor is only supported in Specular mode
                    Texture highColorTex = GetTextureProperty(material, "_HighColor_Tex");
					Texture highColorMask = GetTextureProperty(material, "_Set_HighColorMask");
					if (highColorTex) 
					{ 
						tTexture["_SpecGlossMap"] = highColorTex;
						tTexture["_SpecularDetailMask"] = highColorMask; 
					} else { 
						tTexture["_SpecGlossMap"] = highColorMask; 
					};

                    tFloat["_SpecularType"] = (float)SpecularType.Cel * GetFloatProperty(material, "_Is_SpecularToHighColor") ?? 0 ;

                    tColor["_SpecColor"] = new Vector4(1,1,1,0.1f) * GetColorProperty(material, "_HighColor") ?? (Color.white);

                    float? smoothness = GetFloatProperty(material, "_HighColor_Power");
                    if (smoothness.HasValue) tFloat["_Smoothness"] = 1.0f - smoothness;
					tFloat["_CelSpecularSoftness"] = GetFloatProperty(material, "_Is_SpecularToHighColor");

                    // Rim lighting works differently here, but there's not much we can do about it. 
					tFloat["_UseFresnel"] = (float)AmbientFresnelType.Lit * GetFloatProperty(material, "_RimLight") ?? 0;
                    tColor["_FresnelTint"] = GetColorProperty(material, "_RimLightColor");
					tFloat["_FresnelWidth"] = GetFloatProperty(material, "_RimLight_Power") ?? 0 * 10;
                    tFloat["_FresnelStrength"] = 1.0f - GetFloatProperty(material, "_RimLight_FeatherOff") ?? 0;

                    tFloat["_UseFresnelLightMask"] = GetFloatProperty(material, "_LightDirection_MaskOn");
                    tFloat["_FresnelLightMask"] = 1.0f + GetFloatProperty(material, "_Tweak_LightDirection_MaskLevel") ?? 0;
                    //GetFloatProperty(material, "_Add_Antipodean_RimLight");
                    tColor["_FresnelTintInv"] = GetColorProperty(material, "_Ap_RimLightColor");
                    tFloat["_FresnelWidthInv"] = 10 * GetFloatProperty(material, "_Ap_RimLight_Power") ?? 0;
                    tFloat["_FresnelStrengthInv"] = 1.0f - GetFloatProperty(material, "_Ap_RimLight_FeatherOff") ?? 0;
                    //GetTextureProperty(material, "_Set_RimLightMask");

                    // Matcap properties are not fully supported
                    tFloat["_UseMatcap"] = GetFloatProperty(material, "_MatCap");
                    tTexture["_Matcap1"] = GetTextureProperty(material, "_MatCap_Sampler");
                    tTexture["_MatcapMask"] = GetTextureProperty(material, "_Set_MatcapMask");
                    tColor["_Matcap1Tint"] = GetColorProperty(material, "_MatCapColor");
                    // _Is_LightColor_MatCap is not supported.
                    tFloat["_Matcap1Blend"] = 1.0f - GetFloatProperty(material, "_Is_BlendAddToMatCap") ?? 0;
					// This seems to be used as a strength setting.
					tFloat["_Matcap1Strength"] = 1.0f - GetFloatProperty(material, "_Tweak_MatcapMaskLevel");
                    // _Tweak_MatCapUV, _Rotate_MatCapUV are not yet supported.
                    // _Is_NormalMapForMatCap, _NormalMapForMatCap, _BumpScaleMatcap, _Rotate_NormalMapForMatCapUV
                    // are not supported.

                    tFloat["_OutlineMode"] = 1.0f;
                    if (oldShader.name.Contains("NoOutline"))
                    {
                    	tFloat["_OutlineMode"] = 0.0f;
                	}
                    tColor["_outline_color"] = GetColorProperty(material, "_Outline_Color");
            		tFloat["_outline_width"] = 0.1f * GetFloatProperty(material, "_Outline_Width") ?? 1.0f;
					tFloat["_OutlineZPush"] = GetFloatProperty(material, "_Offset_Z");
            		tTexture["_OutlineMask"] = GetTextureProperty(material, "_Outline_Sampler");

                    // Stencil properties
                    if (oldShader.name.Contains("StencilMask"))
                    {
                    	//Debug.Log(GetIntProperty(material, "_StencilNo") % 256);
                    	stencilReference = (int)GetIntProperty(material, "_StencilNo") % 256;
                    	stencilComparison = (int)CompareFunction.Always;
                    	stencilOperation = (int)StencilOp.Replace;
						stencilFail = (int)StencilOp.Replace;

                	}
                    if (oldShader.name.Contains("StencilOut"))
                    {
                    	//Debug.Log(GetIntProperty(material, "_StencilNo") % 256);
                    	stencilReference = (int)GetIntProperty(material, "_StencilNo") % 256;
                    	stencilComparison = (int)CompareFunction.NotEqual;
                    	stencilOperation = (int)StencilOp.Keep;
						stencilFail = (int)StencilOp.Keep;
                	}

					// Transparency modes
	            	if (oldShader.name.Contains("Clipping"))
	                {
						// Treat Clipping as cutout
	                    tFloat[BaseStyles.renderingModeName] = (float)RenderingMode.Cutout;
                    	tFloat[BaseStyles.customRenderingModeName] = (float)CustomRenderingMode.Cutout;
	                }
	            	if (oldShader.name.Contains("TransClipping"))
	                {
						// TransClipping mode depends on a depth prepass with cutout
						// This is difficult to support and would have low performance, and more importantly,
						// alpha to coverage can replicate it pretty well, so set to cutout.
	                    tFloat[BaseStyles.renderingModeName] = (float)RenderingMode.Cutout;
                    	tFloat[BaseStyles.customRenderingModeName] = (float)CustomRenderingMode.Cutout;
	                }
	            	if (oldShader.name.Contains("Transparent"))
	                {
						// Treat Transparent mode as Fade transparency.
	                    tFloat[BaseStyles.renderingModeName] = (float)RenderingMode.Fade;
                    	tFloat[BaseStyles.customRenderingModeName] = (float)CustomRenderingMode.Fade;
	                }
					
                }
            	if (oldShader.name.Contains("Reflex Shader 2"))
                {
					// Todo
				}
            }

			float? outlineMode = tFloat["_OutlineMode"];
			float? specularType = tFloat["_SpecularType"];

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            // Apply old shader properties to the new shader.
            SetVectorProperty(material, "_MainTex_ST", textureScaleOffset);
            SetShaderFeatureActive(material, null, "_CullMode", cullMode);

			// Assign gathered properties. 
			foreach (KeyValuePair<string, Color?> e in tColor)    { SetColorProperty(material, e.Key, e.Value); };
			foreach (KeyValuePair<string, float?> e in tFloat)    { SetFloatProperty(material, e.Key, e.Value); };
			foreach (KeyValuePair<string, Texture> e in tTexture) { material.SetTexture(e.Key, e.Value); };

			SetIntProperty(material, "_Stencil", stencilReference);
			SetIntProperty(material, "_StencilComp", stencilComparison);
			SetIntProperty(material, "_StencilOp", stencilOperation);
			SetIntProperty(material, "_StencilFail", stencilFail);

            if (outlineMode.HasValue) SetupMaterialWithOutlineMode(material, (OutlineMode)outlineMode);
			if (specularType.HasValue) SetupMaterialWithSpecularType(material, (SpecularType)specularType);

            // Setup the rendering mode based on the old shader.
            if (oldShader == null || !oldShader.name.Contains(LegacyShadersPath))
            {
                SetupMaterialWithRenderingMode(material, (RenderingMode)material.GetFloat(BaseStyles.renderingModeName), CustomRenderingMode.Opaque, -1);
            }
			else
            {
                MaterialChanged(material);
            }
        }

		protected static void SetupMaterialWithAlbedo(Material material, MaterialProperty albedoMap, MaterialProperty albedoAlphaMode)
		{
			switch ((AlbedoAlphaMode)albedoAlphaMode.floatValue)
			{
				case AlbedoAlphaMode.Transparency:
				{
					material.DisableKeyword(CommonStyles.albedoMapAlphaSmoothnessName);
				}
				break;

				case AlbedoAlphaMode.Smoothness:
				{
					material.EnableKeyword(CommonStyles.albedoMapAlphaSmoothnessName);
				}
				break;
			}
		}

		protected static void SetupMaterialWithOutlineMode(Material material, OutlineMode outlineMode)
		{
			string[] oldShaderName = material.shader.name.Split('/');
            const string outlineName = " (Outline)"; //
            var currentlyOutline = oldShaderName[oldShaderName.Length - 1].Contains(outlineName);
            switch ((OutlineMode)outlineMode)
            {
            	case OutlineMode.None:
            	if (currentlyOutline) {
            		string[] newShaderName = oldShaderName;
            		string newSubShader = oldShaderName[oldShaderName.Length - 1].Replace(outlineName, "");
            		newShaderName[oldShaderName.Length - 1] = newSubShader;
            		Shader finalShader = Shader.Find(String.Join("/", newShaderName));
                    // If we can't find it, pass.
            		if (finalShader != null) {
            			material.shader = finalShader;
            		}
            	}
            	break;
            	case OutlineMode.Tinted:
            	case OutlineMode.Colored:
            	if (!currentlyOutline) {
            		string[] newShaderName = oldShaderName;
            		string newSubShader = oldShaderName[oldShaderName.Length - 1] + outlineName;
            		newShaderName[oldShaderName.Length - 1] = newSubShader;
            		Shader finalShader = Shader.Find(String.Join("/", newShaderName));
                    // If we can't find it, pass.
            		if (finalShader != null) {
            			material.shader = finalShader;
            		}
            	}
            	break;
            }
        }

        protected static void SetupMaterialWithSpecularType(Material material, SpecularType specularType)
        {
            // Note: _METALLICGLOSSMAP is used to avoid keyword problems with VRchat.
            // It's only a coincidence that the metallic map needs to be present.
            // Note: _SPECGLOSSMAP is used to switch to a version that doesn't sample
            // reflection probes. 
        	switch ((SpecularType)material.GetFloat("_SpecularType"))
        	{
        		case SpecularType.Standard:
        		material.SetFloat("_SpecularType", 1);
        		material.EnableKeyword("_METALLICGLOSSMAP");
        		material.DisableKeyword("_SPECGLOSSMAP");
        		break;
        		case SpecularType.Cloth:
        		material.SetFloat("_SpecularType", 2);
        		material.EnableKeyword("_METALLICGLOSSMAP");
        		material.DisableKeyword("_SPECGLOSSMAP");
        		break;
        		case SpecularType.Anisotropic:
        		material.SetFloat("_SpecularType", 3);
        		material.EnableKeyword("_METALLICGLOSSMAP");
        		material.DisableKeyword("_SPECGLOSSMAP");
        		break;
        		case SpecularType.Cel:
        		material.SetFloat("_SpecularType", 4);
        		material.EnableKeyword("_SPECGLOSSMAP");
        		material.DisableKeyword("_METALLICGLOSSMAP");
        		break;
        		case SpecularType.CelStrand:
        		material.SetFloat("_SpecularType", 5);
        		material.EnableKeyword("_SPECGLOSSMAP");
        		material.DisableKeyword("_METALLICGLOSSMAP");
        		break;
        		case SpecularType.Disable:
        		material.SetFloat("_SpecularType", 0);
        		material.DisableKeyword("_METALLICGLOSSMAP");
        		material.DisableKeyword("_SPECGLOSSMAP");
        		break;
        		default:
        		break;
        	}
        }

        protected static void SetupMaterialWithVertexColorType(Material material, VertexColorType vertexColorType)
        {
        	switch ((VertexColorType)material.GetFloat("_VertexColorType"))
        	{
        		case VertexColorType.Color:
        		material.SetFloat("_VertexColorType", 0);
        		break;
        		case VertexColorType.OutlineColor:
        		material.SetFloat("_VertexColorType", 1);
        		break;
        		case VertexColorType.AdditionalData:
        		material.SetFloat("_VertexColorType", 2);
        		break;
        		case VertexColorType.Ignore:
        		material.SetFloat("_VertexColorType", 3);
        		break;
        		default:
        		break;
        	}
        }
        
        protected static void SetupMaterialWithLightingCalculationType(Material material, LightingCalculationType LightingCalculationType)
        {
        	switch ((LightingCalculationType)material.GetFloat("_LightingCalculationType"))
        	{   
        		case LightingCalculationType.Standard:
        		material.SetFloat("_LightingCalculationType", 1);
        		break;
        		case LightingCalculationType.Cubed:
        		material.SetFloat("_LightingCalculationType", 2);
        		break;
        		case LightingCalculationType.Directional:
        		material.SetFloat("_LightingCalculationType", 3);
        		break;
        		case LightingCalculationType.Biased:
        		material.SetFloat("_LightingCalculationType", 4);
        		break;
        		default:
        		case LightingCalculationType.Unbiased:
        		material.SetFloat("_LightingCalculationType", 0);
        		break;
        	}
        }

	    protected static void UpgradeVariantCheck(Material material)
	    {
	        const string oldNoOutlineName = "☓ No Outline";
	        string newShaderName = "Silent's Cel Shading/Lightramp";
	        const string upgradeNotice = 
	        "Note: Updated the shader for material {0} to the new format.";
	        string[] currentShaderName = material.shader.name.Split('/');
	        // If they're the No Outline variant, it'll be in the path
	        var currentlyNoOutline = currentShaderName[currentShaderName.Length - 2].Equals(oldNoOutlineName);
	        newShaderName = currentlyNoOutline
	        ? newShaderName
	        : newShaderName + " (Outline)";
	        // Old shaders start with "Hidden/Silent's Cel Shading Shader/Old/"
	        if (currentShaderName[0] == "Hidden" && currentShaderName[2] == "Old")
	        {
	            // SetupMaterialWithRenderingMode
	            Shader finalShader = Shader.Find(newShaderName);
	            // If we can't find it, pass.
	            if (finalShader != null) {
	                material.shader = finalShader;
	                Debug.Log(String.Format(upgradeNotice, material.name, material.shader.name, newShaderName));
	            }
	        }
	    }

	}
}