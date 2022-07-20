// Derived from https://github.com/Xiexe/Xiexes-Unity-Shaders
// with Xiexe's permission. For compatibility's sake, though,
// I've kept the namespaces seperate but similar. 

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using UnityEditorInternal;
using static SilentCelShading.Unity.InspectorCommon;

namespace SilentCelShading.Unity
{
public class XSGradientEditor : EditorWindow
{

    private static int gradients_min = 1;
    private static int gradients_max = 8;

    public List<int> gradients_index = new List<int>(new int[1] { 0 });
    public List<Gradient> gradients = new List<Gradient>(gradients_max);
    public Texture2D tex;

    private string finalFilePath;

    private bool isLinear = false;
    private bool manualMaterial = false;
    private enum Resolutions
    {
        Tiny64x8 = 64,
        Small128x8 = 128,
        Medium256x8 = 256,
        Large512x8 = 512
    }
    private Resolutions res = Resolutions.Tiny64x8;
    public static Material focusedMat;
    private Material oldFocusedMat;
    private Texture oldTexture;
    private string rampProperty = "_Ramp";
    private ReorderableList grad_index_reorderable;
    private bool reorder;
    private static GUIContent iconToolbarPlus;
    private static GUIContent iconToolbarMinus;
    private static GUIStyle preButton;
    private static GUIStyle buttonBackground;
    private bool changed;
    private int loadGradIndex;
    private SCSSMultiGradient multiGrad;
    private Vector2 scrollPos;
    public string currentMatName;

    private bool dHelpText = true;


    protected GUIContent GetInspectorGUIContent(string i)
    {
        GUIContent style;
        if (!styles.TryGetValue(i, out style))
        {
            style = new GUIContent(i);
        }
        return style;
    }

    protected string GetInspectorData(string i)
    {
        GUIContent style;
        if (!styles.TryGetValue(i, out style))
        {
            return i;
        }
        return style.text;
    }

    [MenuItem("Tools/Silent's Cel Shading/Gradient Editor")]
    static public void Init()
    {
        XSGradientEditor window = EditorWindow.GetWindow<XSGradientEditor>(false, "SCSS Gradient Editor", true);
        window.minSize = new Vector2(450, 390);
    }

    //Find Asset Path
    public static string findAssetPath(string finalFilePath)
    {
        string[] guids1 = AssetDatabase.FindAssets("SCSS_XSGradientEditor", null);
        string untouchedString = AssetDatabase.GUIDToAssetPath(guids1[0]);
        string[] splitString = untouchedString.Split('/');

        ArrayUtility.RemoveAt(ref splitString, splitString.Length - 1);
        ArrayUtility.RemoveAt(ref splitString, splitString.Length - 1);

        finalFilePath = string.Join("/", splitString);
        return finalFilePath;
    }

    public void OnGUI()
    {
        changed = false;
        EditorGUILayout.Space();

        currentMatName = (focusedMat != null)
        ? focusedMat.name 
        : "None";

        GUILayout.Label(GetInspectorData("ge_gradientEditorTitle") + " " + currentMatName, EditorStyles.boldLabel, new GUILayoutOption[0]);

        if (preButton == null)
        {
            iconToolbarPlus = EditorGUIUtility.IconContent("Toolbar Plus", GetInspectorData("ge_addButton"));
            iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", GetInspectorData("ge_removeButton"));
            preButton = new GUIStyle("RL FooterButton");
            buttonBackground = new GUIStyle("RL Header");
        }

        if (gradients.Count < gradients_max)
        {
            for (int i = gradients.Count; i < gradients_max; i++)
            {
            gradients.Add(new Gradient());
            }
        }

        if (grad_index_reorderable == null)
        {
            makeReorderedList();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        Rect r = EditorGUILayout.GetControlRect();
        float rightEdge = r.xMax;
        float leftEdge = rightEdge - 48f;
        r = new Rect(leftEdge, r.y, rightEdge - leftEdge, r.height);
        if (Event.current.type == EventType.Repaint) buttonBackground.Draw(r, false, false, false, false);
        leftEdge += 18f;
        EditorGUI.BeginDisabledGroup(gradients_index.Count >= gradients_max);
        bool addE = GUI.Button(new Rect(leftEdge + 4, r.y, 25, 13), iconToolbarPlus, preButton);
        EditorGUI.EndDisabledGroup();
        EditorGUI.BeginDisabledGroup(gradients_index.Count <= gradients_min);
        bool removeE = GUI.Button(new Rect(leftEdge - 19, r.y, 25, 13), iconToolbarMinus, preButton);
        EditorGUI.EndDisabledGroup();

        if (addE)
        {
            grad_index_reorderable.index++;
            int wat = 0;
            for (int i = 0; i < gradients_max; i++)
            {
                if (!gradients_index.Contains(i))
                {
                    wat = i;
                    break;
                }
            }
            gradients_index.Add(wat);
            changed = true;
        }
        if (removeE)
        {
            gradients_index.Remove(gradients_index[gradients_index.Count - 1]);
            grad_index_reorderable.index--;
            changed = true;
        }

        GUIStyle button = new GUIStyle(EditorStyles.miniButton);
        button.normal = !reorder ? EditorStyles.miniButton.normal : EditorStyles.miniButton.onNormal;
        if (GUILayout.Button(GetInspectorGUIContent("ge_reorderButton"), button, GUILayout.ExpandWidth(false)))
        {
            reorder = !reorder;
        }
        GUILayout.EndHorizontal();

        SerializedObject serializedObject = new SerializedObject(this);
        if (reorder)
        {
            grad_index_reorderable.DoLayoutList();
        }
        else
        {
            SerializedProperty colorGradients = serializedObject.FindProperty("gradients");
            if (colorGradients.arraySize == gradients_max)
            {
                for (int i = 0; i < gradients_index.Count; i++)
                {
                    Rect _r = EditorGUILayout.GetControlRect();
                    _r.x += 8f;
                    _r.width -= 2f * 8f;
                    _r.height += 5f;
                    _r.y += 2f + (3f * i);
                    EditorGUI.PropertyField(_r, colorGradients.GetArrayElementAtIndex(gradients_index[i]), new GUIContent(""));
                }
                GUILayout.Space(9 + gradients_index.Count*3);
            }
        }
        if (serializedObject.ApplyModifiedProperties()) changed = true;

        if (oldFocusedMat != focusedMat)
        {
            changed = true;
            if (this.oldTexture != null)
            {
                if (this.oldTexture == EditorGUIUtility.whiteTexture) this.oldTexture = null;
                oldFocusedMat.SetTexture(rampProperty, this.oldTexture);
                this.oldTexture = null;
            }
            oldFocusedMat = focusedMat;
        }

        Resolutions oldRes = res;
        res = (Resolutions)EditorGUILayout.EnumPopup(GetInspectorGUIContent("ge_resolutionTitle"), res);
        if (oldRes != res) changed = true;

        int width = (int)res;
        int height = 8; // Todo: Add 16 option
        if (tex == null)
        {
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        }

        EditorGUILayout.Space();
        bool old_isLinear = isLinear;
        drawAdvancedOptions();
        if (old_isLinear != isLinear)
        {
            changed = true;
        }

        if (manualMaterial)
        {
            focusedMat = (Material)EditorGUILayout.ObjectField(new GUIContent("", ""), focusedMat, typeof(Material), true);
        }

        if (focusedMat != null)
        {
            if (focusedMat.HasProperty("_Ramp"))
            {
                rampProperty = "_Ramp";
            }
            else
            {
                rampProperty = EditorGUILayout.TextField(GetInspectorGUIContent("ge_rampPropertyField"), rampProperty);
                if (!focusedMat.HasProperty(rampProperty))
                {
                    GUILayout.Label(GetInspectorGUIContent("ge_rampPropertyError"));
                }
            }
        }

        if (changed)
        {
            updateTexture(width, height);
            if (focusedMat != null)
            {
                if (focusedMat.HasProperty(rampProperty))
                {
                    if (this.oldTexture == null)
                    {
                        if (focusedMat.GetTexture(rampProperty) == null)
                        {
                            this.oldTexture = EditorGUIUtility.whiteTexture;
                        }
                        else
                        {
                            this.oldTexture = focusedMat.GetTexture(rampProperty);
                        }
                    }
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply(false, false);
                    focusedMat.SetTexture(rampProperty, tex);
                }
            }
        }

        EditorGUILayout.Space();
        drawMGInputOutput();

        EditorGUILayout.Space();
        if (GUILayout.Button(GetInspectorGUIContent("ge_saveRampButton")))
        {
            finalFilePath = findAssetPath(finalFilePath);
            string path = EditorUtility.SaveFilePanel(GetInspectorData("ge_saveRampButton"), finalFilePath + "/Textures/Shadow Ramps/Generated", "gradient", "png");
            if (path.Length != 0)
            {
                updateTexture(width, height);
                bool success = GenTexture(tex, path);
                if (success)
                {
                    if (focusedMat != null)
                    {
                        string s = path.Substring(path.IndexOf("Assets"));
                        Texture ramp = AssetDatabase.LoadAssetAtPath<Texture>(s);
                        if (ramp != null)
                        {
                            focusedMat.SetTexture(rampProperty, ramp);
                            this.oldTexture = null;
                        }
                    }
                }
            }
        }
        drawHelpText();
    }   

    Gradient reflessGradient(Gradient old_grad)
    {
        Gradient grad = new Gradient();
        grad.SetKeys(old_grad.colorKeys, old_grad.alphaKeys);
        grad.mode = old_grad.mode;
        return grad;
    }

    List<int> reflessIndexes(List<int> old_indexes)
    {
        List<int> indexes = new List<int>();
        for (int i = 0; i < old_indexes.Count; i++)
        {
            indexes.Add(old_indexes[i]);
        }
        return indexes;
    }

    void makeReorderedList()
    {
        grad_index_reorderable = new ReorderableList(gradients_index, typeof(int), true, false, false, false);
        grad_index_reorderable.headerHeight = 0f;
        grad_index_reorderable.footerHeight = 0f;
        grad_index_reorderable.showDefaultBackground = true;

        grad_index_reorderable.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            if (gradients.Count == gradients_max)
            {
                Type editorGui = typeof(EditorGUI);
                MethodInfo mi = editorGui.GetMethod("GradientField", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[2] { typeof(Rect), typeof(Gradient) }, null);
                mi.Invoke(this, new object[2] { rect, gradients[gradients_index[index]] });
                if (Event.current.type == EventType.Repaint)
                {
                    changed = true;
                }
            }
        };

        grad_index_reorderable.onChangedCallback = (ReorderableList list) =>
        {
            changed = true;
        };
    }

    void OnDestroy()
    {
        if (focusedMat != null)
        {
            if (this.oldTexture != null)
            {
                if (this.oldTexture == EditorGUIUtility.whiteTexture)
                {
                    this.oldTexture = null;
                }
                focusedMat.SetTexture(rampProperty, this.oldTexture);
                this.oldTexture = null;
            }
            focusedMat = null;
        }
    }

    void updateTexture(int width, int height)
    {
        tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++) // Per gradient
        {
            for (int x = 0; x < width; x++) // Per pixel
            {
                Color grad_col = gradients[gradients_index[Mathf.Min(y, gradients_index.Count-1)]].Evaluate((float)x / (float)width);
                tex.SetPixel(x, y, isLinear ? grad_col.gamma : grad_col);
                
            }
        }
    }

    bool GenTexture(Texture2D tex, string path)
    {
        var pngData = tex.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();
            return ChangeImportSettings(path);
        }
        return false;
    }

    bool ChangeImportSettings(string path)
    {

        string s = path.Substring(path.LastIndexOf("Assets"));
        TextureImporter texture = (TextureImporter)TextureImporter.GetAtPath(s);
        if (texture != null)
        {
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.maxTextureSize = 512;
            texture.mipmapEnabled = false;
            texture.textureCompression = TextureImporterCompression.Uncompressed;

            // texture.sRGBTexture = !isLinear; // We already do the conversion in tex.SetPixel

            texture.SaveAndReimport();
            AssetDatabase.Refresh();
            return true;

            // shadowRamp = (Texture)Resources.Load(path);
            // Debug.LogWarning(shadowRamp.ToString());
        }
        else
        {
            Debug.Log(GetInspectorGUIContent("ge_noAssetPathError"));
        }
        return false;
    }

    void drawMGInputOutput()
    {
        GUILayout.BeginHorizontal();
        SCSSMultiGradient old_multiGrad = multiGrad;
        multiGrad = (SCSSMultiGradient)EditorGUILayout.ObjectField(GetInspectorGUIContent("ge_multiGradientPreset"), multiGrad, typeof(SCSSMultiGradient), false, null);
        if (multiGrad != old_multiGrad)
        {
            if (multiGrad != null)
            {
                this.gradients = multiGrad.gradients;
                this.gradients_index = multiGrad.order;
                makeReorderedList();
            }
            else
            {
                List<Gradient> new_Grads = new List<Gradient>();
                for (int i = 0; i < this.gradients.Count; i++)
                {
                    new_Grads.Add(reflessGradient(this.gradients[i]));
                }
                this.gradients = new_Grads;
                this.gradients_index = reflessIndexes(this.gradients_index);
                makeReorderedList();
            }
            changed = true;
        }

        if (GUILayout.Button(GetInspectorGUIContent("ge_saveNewButton"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
        {
            finalFilePath = findAssetPath(finalFilePath);
            string path = EditorUtility.SaveFilePanel(GetInspectorData("ge_saveMultiGradient"), (finalFilePath + "/Textures/Shadow Ramps/MGPresets"), "MultiGradient", "asset");
            if (path.Length != 0)
            {
                path = path.Substring(Application.dataPath.Length - "Assets".Length);
                SCSSMultiGradient _multiGrad = ScriptableObject.CreateInstance<SCSSMultiGradient>();
                _multiGrad.uniqueName = Path.GetFileNameWithoutExtension(path);
                foreach (Gradient grad in gradients)
                {
                    _multiGrad.gradients.Add(reflessGradient(grad));
                }
                _multiGrad.order.AddRange(gradients_index.ToArray());
                multiGrad = _multiGrad;
                AssetDatabase.CreateAsset(_multiGrad, path);
                this.gradients = multiGrad.gradients;
                this.gradients_index = multiGrad.order;
                makeReorderedList();
                AssetDatabase.SaveAssets();
            }
        }
        GUILayout.EndHorizontal();
    }

    void drawAdvancedOptions()
    {
        GUILayout.BeginHorizontal();
        isLinear = GUILayout.Toggle(isLinear, GetInspectorGUIContent("ge_linearCheckbox"));
        manualMaterial = GUILayout.Toggle(manualMaterial, GetInspectorGUIContent("ge_materialCheckbox"));
        dHelpText = GUILayout.Toggle(dHelpText, GetInspectorGUIContent("ge_helpCheckbox"));
        GUILayout.EndHorizontal();
    }

    void drawHelpText()
    {
        if(dHelpText)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(GetInspectorData("ge_basicHelp"), MessageType.Info);
            EditorGUILayout.HelpBox(GetInspectorData("ge_multiRampHelp"), MessageType.Info);
        }
    }

    // External

    static public void callGradientEditor(Material focusedMat = null)
    {
            XSGradientEditor.focusedMat = focusedMat;
            XSGradientEditor.Init();
    }

}
}