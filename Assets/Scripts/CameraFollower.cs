using Cinemachine;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private new CinemachineVirtualCamera camera;

    public void Start()
    {
        camera = FindObjectOfType<CinemachineVirtualCamera>();
    }

    public void SwitchFollow(Transform transform)
    {
        camera.Follow = transform;
    }
}
