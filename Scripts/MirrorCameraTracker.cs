// SPDX-License-Identifier: MIT
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;
using UdonSharp;


namespace MMMaellon
{
    [RequireComponent(typeof(VRCMirrorReflection))]
    public class MirrorCameraTracker : UdonSharpBehaviour
    {
        public Camera target;
        // public GameObject left_eye_object;
        // public GameObject right_eye_object;
        // public Camera other_cam;
        public RenderTexture render_texture;
        public UnityEngine.UI.Text debugText;

        void Start()
        {
            target.enabled = true; // enable camera because vrc disables it on load
            render_texture.vrUsage = UnityEngine.VRTextureUsage.TwoEyes;
        }

        string debugStats;
        float minEyeOffset = -1;
        void LateUpdate()
        {
            debugStats += $"minEyeOffset={minEyeOffset:F8}\n";
            var headTracker = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            SetCameraTransform(target, headTracker.position, headTracker.rotation, minEyeOffset);
            debugStats += $"cameraScale={target.transform.parent.lossyScale.x:F8}\n";
            if (debugText)
                debugText.text = debugStats;
            debugStats = "";
            minEyeOffset = -1;
        }

        const float minOffsetXWeight = 0.9f;
        Camera mirrorCam;
        Vector3 mirrorHeadPos;
        Vector3 mirrorHeadRight;
        int lastFrameCount;
        void OnWillRenderObject()
        {
            if (!mirrorCam)
            {
                mirrorCam = GameObject.Find("/MirrorCam" + gameObject.name).GetComponent<Camera>();
                if (!mirrorCam)
                    return;
            }
            var frameCount = Time.frameCount;
            if (lastFrameCount != frameCount)
            {
                lastFrameCount = frameCount;
                var headTracker = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                var mirrorCenter = transform.position;
                var mirrorNormal = transform.forward;
                mirrorHeadPos = Vector3.Reflect(headTracker.position - mirrorCenter, mirrorNormal) + mirrorCenter;
                mirrorHeadRight = Vector3.Reflect(headTracker.rotation * Vector3.right, mirrorNormal);
                if (Vector3.Dot(mirrorNormal, headTracker.position - mirrorCenter) > 0)
                    mirrorHeadRight = Vector3.zero; // back view doesn't move the camera
            }
            var eyeOffset = mirrorCam.transform.position - mirrorHeadPos;
            if (Mathf.Abs(Vector3.Dot(eyeOffset, mirrorHeadRight)) >= eyeOffset.magnitude * minOffsetXWeight)
            {
                if (minEyeOffset < 0)
                {
                    minEyeOffset = eyeOffset.magnitude;
                }
                else
                {
                    minEyeOffset = Mathf.Min(minEyeOffset, eyeOffset.magnitude);
                }
            }

            // MeshRenderer renderer = (MeshRenderer)GetComponent(typeof(MeshRenderer));
            // renderer.enabled = false;

            // other_cam.gameObject.SetActive(true);
        }

        const float maxScale = 10f;
        void SetCameraTransform(Camera camera, Vector3 position, Quaternion rotation, float eyeOffset)
        {
            var cameraT = camera.transform;
            var playspace = cameraT.parent;
            var scale0 = playspace.localScale.x;
            // reset transform to avoid breaking stereoViewMatrix
            playspace.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            playspace.localScale = Vector3.one;
            if (!camera.stereoEnabled)
                cameraT.SetPositionAndRotation(position, rotation);
            else
            {
                // note: camera.stereoSeparation/2 is inaccurate!
                var baseEyeOffset = camera.GetStereoViewMatrix(Camera.StereoscopicEye.Left)
                        .MultiplyPoint3x4(cameraT.position).magnitude;
                if (baseEyeOffset == 0)
                {
                    return;
                }
                debugStats += $"baseEyeOffset={baseEyeOffset:F8}\n";
                var scale = eyeOffset / baseEyeOffset;
                if (scale > maxScale)
                    scale = scale0;
                var rot = rotation * Quaternion.Inverse(cameraT.localRotation);
                var pos = position - rot * cameraT.localPosition * scale;
                playspace.SetPositionAndRotation(pos, rot);
                playspace.localScale = scale * Vector3.one;
                // left_eye_object.transform.rotation = rotation;
                // right_eye_object.transform.rotation = rotation;
                // left_eye_object.transform.position = cameraT.position + cameraT.rotation * Vector3.left * eyeOffset;
                // right_eye_object.transform.position = cameraT.position + cameraT.rotation * Vector3.right * eyeOffset;
            }
        }
    }
}