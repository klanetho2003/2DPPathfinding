using Unity.Cinemachine;
using UnityEngine;

public class CCameraController : MonoBehaviour
{
    void Start()
    {
        if (TrySetTarget() == false)
        {
            Debug.LogWarning("Player Cant Found. Check in Managers.Object.Player");
        }
    }

    bool TrySetTarget()
    {
        GameObject player = Managers.Object.Player.gameObject;

        if (player.IsValid() == false)
            return false;

        // Set Tracking Target
        CinemachineCamera cam = GetComponent<CinemachineCamera>();
        cam.Follow = player.transform;

        return true;
    }
}
