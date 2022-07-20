
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Scope : UdonSharpBehaviour
{
    public MeshRenderer lens_mesh;
    public P_Shooter gunObject;
    public Transform gunMesh;
    public Cinemachine.CinemachineVirtualCamera scope_camera;
    public float zoom_amount = 2.0f;
    public bool render_as_2d_screen = false;
    private float last_scope = 0;
    private float last_unscope = 0;
    private bool start_ran = false;
    private Vector3 last_pos = Vector3.zero;
    private Quaternion last_rot = Quaternion.identity;
    private Vector3 local_rest_pos = Vector3.zero;
    private Quaternion local_rest_rot = Quaternion.identity;
    void Start()
    {
        if (scope_camera != null)
        {
            scope_camera.transform.localPosition = Vector3.zero;
            scope_camera.transform.localRotation = Quaternion.identity;
            scope_camera.gameObject.SetActive(false);
        }
        if (gunMesh != null)
        {
            transform.parent = gunMesh;
        }
        if (gunObject != null)
        {
            local_rest_pos = Quaternion.Inverse(transform.rotation) * (transform.position - gunObject.transform.position);
            local_rest_rot = Quaternion.Inverse(gunObject.transform.rotation) * (transform.rotation);
        }
        start_ran = true;
    }

    public void Zoom()
    {
        if (start_ran && gunObject != null && gunObject.local_held)
        {
            Quaternion player_rot = Networking.LocalPlayer.GetRotation();
            Vector3 player_pos = Networking.LocalPlayer.GetPosition();

            last_rot = player_rot * last_rot;
            last_pos = player_pos + player_rot * last_pos;

            Vector3 target_pos = Vector3.zero;
            Quaternion target_rot = Quaternion.identity;
            Vector3 parent_pos = Vector3.zero;
            Quaternion parent_rot = Quaternion.identity;
            if (gunObject.transform.parent != null)
            {
                parent_pos = gunObject.transform.parent.position;
                parent_rot = gunObject.transform.parent.rotation;
            }
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                VRCPlayerApi.TrackingData headData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                target_rot = headData.rotation;
                target_pos = headData.position + headData.rotation * Vector3.forward * 0.15f;
                if (last_unscope + 1f > Time.timeSinceLevelLoad)
                {
                    float lerp_amount = Mathf.Lerp(1f, 0f, (last_unscope + 1f) - Time.timeSinceLevelLoad);
                    target_rot = Quaternion.Slerp(last_rot, target_rot, lerp_amount);
                    target_pos = Vector3.Lerp(last_pos, target_pos, lerp_amount);
                }
                last_scope = Time.timeSinceLevelLoad;
            } else if (gunObject.grip == null || !gunObject.grip.reparented)
            {
                target_pos = parent_pos;
                target_rot = parent_rot * gunObject.rest_local_rotation * local_rest_rot;
                if (last_scope + 1f > Time.timeSinceLevelLoad)
                {
                    float lerp_amount = Mathf.Lerp(1f, 0f, (last_scope + 1f) - Time.timeSinceLevelLoad);
                    target_rot = Quaternion.Slerp(last_rot, target_rot, lerp_amount);
                    target_pos = Vector3.Lerp(last_pos, target_pos, lerp_amount);
                }
                last_unscope = Time.timeSinceLevelLoad;
            }
            
            if (gunObject.grip == null || !gunObject.grip.reparented)
            {
                gunObject.transform.rotation = target_rot * Quaternion.Inverse(local_rest_rot);
                gunObject.transform.position = target_pos - target_rot * local_rest_pos;
            }

            Quaternion inverse_player_rot = Quaternion.Inverse(player_rot);
            last_rot = inverse_player_rot * target_rot;
            last_pos = inverse_player_rot * (target_pos - player_pos);
        }
    }

    public void ResetZoom()
    {
        if (gunObject != null)
        {
            gunObject.transform.localPosition = Vector3.zero;
            gunObject.transform.localRotation = gunObject.rest_local_rotation;
        }
    }

    public void SetCameraActive(bool camera_enable)
    {
        if (scope_camera != null)
        {
            scope_camera.gameObject.SetActive(camera_enable);
        }

        if (!camera_enable)
        {
            ResetZoom();
        }
    }
}
