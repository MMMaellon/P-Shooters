
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;
using UdonSharpEditor;
using System.Collections.Immutable;
#endif
namespace MMMaellon
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(ScopeManager))]
    public class ScopeManagerEditor : Editor
    {

        public static void SetupScopes()
        {
            int count = 0;
            ScopeManager manager = null;
            foreach (ScopeManager go in GameObject.FindObjectsOfType(typeof(ScopeManager)) as ScopeManager[])
            {
                if (go != null)
                {
                    manager = go;
                    break;
                }
            }
            if (manager == null)
            {
                Debug.LogError($"[<color=#FF00FF>UdonSharp</color>] Could not find a Scope Manager. Did you drag the prefab into your scene?");
                return;
            }

            GunManager gunManager = null;
            foreach (GunManager go in GameObject.FindObjectsOfType(typeof(GunManager)) as GunManager[])
            {
                if (go != null)
                {
                    gunManager = go;
                    break;
                }
            }
            if (gunManager == null)
            {
                Debug.LogError($"[<color=#FF00FF>UdonSharp</color>] Could not find a Gun Manager. Did you drag the prefab into your scene?");
                return;
            }

            SerializedObject gunSerialized = new SerializedObject(gunManager);
            gunSerialized.FindProperty("scope_manager").objectReferenceValue = manager;
            gunSerialized.ApplyModifiedProperties();


            SerializedObject serializedScopeManager = new SerializedObject(manager);
            serializedScopeManager.FindProperty("scopes").ClearArray();
            foreach (Scope scope in GameObject.FindObjectsOfType(typeof(Scope)) as Scope[])
            {
                serializedScopeManager.FindProperty("scopes").InsertArrayElementAtIndex(count);
                serializedScopeManager.FindProperty("scopes").GetArrayElementAtIndex(count).objectReferenceValue = scope;
                if (scope.transform.parent != null && (scope.gunObject == null || scope.gunMesh == null))
                {
                    foreach (P_Shooter shooter in scope.transform.parent.GetComponentsInChildren<P_Shooter>())
                    {
                        SerializedObject scopeSerialized = new SerializedObject(scope);
                        scopeSerialized.FindProperty("gunObject").objectReferenceValue = shooter;
                        scopeSerialized.FindProperty("gunMesh").objectReferenceValue = shooter.transform.Find("model");
                        scopeSerialized.ApplyModifiedProperties();
                        break;
                    }
                }
                if (scope.gunObject != null)
                {
                    SerializedObject serialized = new SerializedObject(scope.gunObject);
                    serialized.FindProperty("scope").objectReferenceValue = scope;
                    serialized.ApplyModifiedProperties();
                    bool missing_ref = true;
                    while (missing_ref)
                    {
                        missing_ref = false;
                        serialized.ApplyModifiedProperties();
                        for (int i = 0; i < scope.gunObject.local_held_objects.Length; i++)
                        {
                            if (scope.gunObject.local_held_objects[i] == null)
                            {
                                missing_ref = true;
                                serialized.FindProperty("local_held_objects").DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                    }
                    serialized.ApplyModifiedProperties();

                    if (scope.lens_mesh != null)
                    {
                        bool already_added = false;
                        foreach (GameObject obj in scope.gunObject.local_held_objects)
                        {
                            if (obj != null && GameObject.ReferenceEquals(obj, scope.lens_mesh.gameObject))
                            {
                                already_added = true;
                                break;
                            }
                        }
                        if (!already_added)
                        {
                            serialized.FindProperty("local_held_objects").InsertArrayElementAtIndex(0);
                            serialized.FindProperty("local_held_objects").GetArrayElementAtIndex(0).objectReferenceValue = scope.lens_mesh.gameObject;
                            serialized.ApplyModifiedProperties();
                        }
                    }
                }
                if (scope.scope_camera != null)
                {
                    SerializedObject serialized_scope_cam = new SerializedObject(scope.scope_camera);
                    if (scope.zoom_amount > 0)
                    {
                        SerializedProperty fov = serialized_scope_cam.FindProperty("m_Lens").FindPropertyRelative("FieldOfView");
                        fov.floatValue = 20f / (scope.zoom_amount / 2f);
                        serialized_scope_cam.ApplyModifiedProperties();
                    }
                    else
                    {
                        SerializedProperty fov = serialized_scope_cam.FindProperty("m_Lens").FindPropertyRelative("FieldOfView");
                        fov.floatValue = 20f;
                        serialized_scope_cam.ApplyModifiedProperties();
                    }
                    scope.scope_camera.transform.localPosition = Vector3.zero;
                    scope.scope_camera.transform.localRotation = Quaternion.identity;
                }
                count++;
            }
            serializedScopeManager.ApplyModifiedProperties();
            if (count > 0)
            {
                Debug.Log($"Set up {count} scopes");
            }
            else
            {
                Debug.Log($"No scopes were set up");
            }
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("SCOPES SETUP");
            EditorGUILayout.HelpBox(
    @"Look at your GunManager Object for instructions", MessageType.Info);
            if (GUILayout.Button(new GUIContent("Set up ALL Scopes")))
            {
                ScopeManagerEditor.SetupScopes();
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
#endif

    [DefaultExecutionOrder(1001)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ScopeManager : UdonSharpBehaviour
    {
        public GunManager gun;
        private VRC_Pickup leftPickupCache;
        private VRC_Pickup rightPickupCache;
        private Scope leftScopeCache;
        private Scope rightScopeCache;

        public Scope[] scopes;
        public Camera scopeCam;
        public void Start()
        {

        }

        public void _Register(GunManager gunManager)
        {
            gun = gunManager;
            foreach (Scope s in scopes)
            {
                if (s != null)
                {
                    s._Register(this);
                }
            }
        }
        public void LateUpdate()
        {
            VRC_Pickup leftPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Left);
            VRC_Pickup rightPickup = Networking.LocalPlayer.GetPickupInHand(VRC_Pickup.PickupHand.Right);
            if (leftPickup != leftPickupCache)
            {
                leftPickupCache = leftPickup;
                if (leftPickup == null)
                {
                    leftScopeCache = null;
                }
                else
                {
                    foreach (Scope scope in leftPickup.GetComponentsInChildren<Scope>())
                    {
                        if (scope != null)
                        {
                            leftScopeCache = scope;
                            break;
                        }
                    }
                }
            }
            if (rightPickup != rightPickupCache)
            {
                rightPickupCache = rightPickup;
                if (rightPickup == null)
                {
                    rightScopeCache = null;
                }
                else
                {
                    foreach (Scope scope in rightPickup.GetComponentsInChildren<Scope>())
                    {
                        if (scope != null)
                        {
                            rightScopeCache = scope;
                            break;
                        }
                    }
                }
            }
            float leftDist = 999;
            float rightDist = 999;
            Vector3 headPos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
            bool scopeActive = false;
            if (leftScopeCache != null)
            {
                leftScopeCache.Zoom();
                leftDist = Vector3.Distance(headPos, leftScopeCache.transform.position);
                leftScopeCache.SetCameraActive(true);
                scopeActive = true;
            }
            if (rightScopeCache != null)
            {
                rightScopeCache.Zoom();
                rightDist = Vector3.Distance(headPos, rightScopeCache.transform.position);
                if (leftScopeCache != null)
                {
                    bool camera_active = rightDist < leftDist;
                    leftScopeCache.SetCameraActive(!camera_active);
                    rightScopeCache.SetCameraActive(camera_active);
                    scopeActive = true;
                }
                else
                {
                    rightScopeCache.SetCameraActive(true);
                    scopeActive = true;
                }
            }
            if (!scopeActive)
            {
                gameObject.SetActive(false);
            }
        }
    }
}