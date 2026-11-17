
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    [Header("Zone Cameras")]
    [SerializeField] private CinemachineCamera zone1Aerial;
    [SerializeField] private CinemachineCamera zone1Orbital;

    [SerializeField] private CinemachineCamera zone2Aerial;
    [SerializeField] private CinemachineCamera zone2Orbital;

    [SerializeField] private CinemachineCamera zone3Aerial;
    [SerializeField] private CinemachineCamera zone3Orbital;

    [Header("Gameplay Cameras")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera centerOrbital;

    [Header("Collectables")]
    [SerializeField] private GameObject[] collectables;

    [Header("Camera Durations")]
    [SerializeField] private float aerialDuration = 5f;
    [SerializeField] private float orbitalDuration = 7f;
    [SerializeField] private float centerDuration = 8f;

    private bool gameplayStarted = false;
    private bool centerSequenceStarted = false;

    private void Start()
    {
        SetAllCamerasPriority(0);

        StartCoroutine(OpeningSequence());
    }

    private void Update()
    {
        if (!gameplayStarted || centerSequenceStarted)
            return;

        if (AllCollectablesDestroyed())
        {
            centerSequenceStarted = true;
            StartCoroutine(CenterSequence());
        }
    }

    private bool AllCollectablesDestroyed()
    {
        foreach (GameObject collectable in collectables)
        {
            if (collectable != null)
                return false;
        }

        return collectables.Length > 0;
    }

    private void SetAllCamerasPriority(int priority)
    {
        zone1Aerial.Priority = priority;
        zone1Orbital.Priority = priority;

        zone2Aerial.Priority = priority;
        zone2Orbital.Priority = priority;

        zone3Aerial.Priority = priority;
        zone3Orbital.Priority = priority;

        playerCamera.Priority = priority;
        centerOrbital.Priority = priority;
    }

    private void SwitchCamera(CinemachineCamera newCamera)
    {
        SetAllCamerasPriority(0);
        newCamera.Priority = 20;
    }

    private IEnumerator OpeningSequence()
    {
        SwitchCamera(zone1Aerial);
        yield return new WaitForSeconds(aerialDuration);

        SwitchCamera(zone1Orbital);
        yield return new WaitForSeconds(orbitalDuration);

        SwitchCamera(zone2Aerial);
        yield return new WaitForSeconds(aerialDuration);

        SwitchCamera(zone2Orbital);
        yield return new WaitForSeconds(orbitalDuration);

        SwitchCamera(zone3Aerial);
        yield return new WaitForSeconds(aerialDuration);

        SwitchCamera(zone3Orbital);
        yield return new WaitForSeconds(orbitalDuration);

        SwitchCamera(playerCamera);

        gameplayStarted = true;
    }

    private IEnumerator CenterSequence()
    {
        SwitchCamera(centerOrbital);

        yield return new WaitForSeconds(centerDuration);

        SwitchCamera(playerCamera);
    }
}
