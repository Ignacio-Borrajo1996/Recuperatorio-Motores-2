using UnityEngine;
using Unity.Cinemachine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;
    [SerializeField] private float rotationSpeed = 20f;

    private void Update()
    {
        orbitalFollow.HorizontalAxis.Value +=
            rotationSpeed * Time.deltaTime;
    }
}