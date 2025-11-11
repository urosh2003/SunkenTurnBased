using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Cinemachine;
using System;

public class CameraActionFocus : MonoBehaviour
{
    [Header("Cameras")]
    public CinemachineVirtualCamera gameplayCam;
    public CinemachineVirtualCamera focusCam;

    [Header("Settings")]
    public CinemachineTargetGroup targetGroup;
    public float focusZoom = 4f;
    public float blendTime = 0.5f;
    public float holdTime = 1f;

    private CameraController controller;
    private Coroutine currentRoutine;
    private TaskCompletionSource<bool> focusTcs;

    public static CameraActionFocus instance;

    [Header("Gameplay Camera Offset")]
    public Vector3 followOffset = new Vector3(0, 2f, 0); // shift up by 2 units on Y
    private Transform offsetTarget;

    private void Awake()
    {
        instance = this;
        controller = FindObjectOfType<CameraController>();

        // Ensure gameplayCam starts active
        gameplayCam.Priority = 10;
        focusCam.Priority = 0;

        // Store its default zoom
        focusCam.m_Lens.OrthographicSize = focusZoom;
        //offsetTarget = new GameObject("CameraOffsetTarget").transform;
    }

    public Task FocusOnSingleAsync(Transform target)
        => StartManagedFocus(new Transform[] { target });

    public Task FocusOnPairAsync(Transform t1, Transform t2)
        => StartManagedFocus(new Transform[] { t1, t2 });

    private Task StartManagedFocus(Transform[] targets)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);


        focusTcs = new TaskCompletionSource<bool>();
        currentRoutine = StartCoroutine(FocusRoutine(targets));
        return focusTcs.Task;
    }

    private IEnumerator FocusRoutine(Transform[] targets)
    {
        controller.LockCamera();

        // Configure target group
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            targetGroup.m_Targets[i].target = targets[i];
            targetGroup.m_Targets[i].weight = 1;
            targetGroup.m_Targets[i].radius = 1;
        }

        // Update offset target to follow group
        //offsetTarget.position = targetGroup.transform.position + followOffset;
        focusCam.LookAt = targetGroup.transform;
        focusCam.Follow = targetGroup.transform;


        // Match focus camera to gameplay camera position/rotation before blending
        focusCam.transform.position = gameplayCam.transform.position;
        focusCam.transform.rotation = gameplayCam.transform.rotation;

        // Switch priorities — Cinemachine Brain will handle blending
        gameplayCam.Priority = 0;
        focusCam.Priority = 10;

        yield return new WaitForSeconds(blendTime + holdTime);

        focusTcs?.TrySetResult(true);
        currentRoutine = null;
    }

    public Task MinigameDone()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        focusTcs = new TaskCompletionSource<bool>();
        currentRoutine = StartCoroutine(BreakFocus());
        return focusTcs.Task;
    }

    private IEnumerator BreakFocus()
    {
        // Switch back to gameplay camera
        focusCam.Priority = 0;
        gameplayCam.Priority = 10;

        gameplayCam.transform.position = focusCam.transform.position;
        focusCam.transform.rotation = focusCam.transform.rotation;

        yield return new WaitForSeconds(blendTime);

        controller.UnlockCamera();
        targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        currentRoutine = null;
        focusTcs?.TrySetResult(true);
    }

    public Task FocusOnPairAsync(Transform t1, Vector3 worldPos)
    {
        // Create a temporary target at the given position
        GameObject tempTarget = new GameObject("TempFocusPoint");
        tempTarget.transform.position = worldPos;

        // Start focus routine, and clean up afterward
        var task = StartManagedFocus(new Transform[] { t1, tempTarget.transform });

        // Destroy temp target once focus ends
        _ = task.ContinueWith(_ => Destroy(tempTarget));

        return task;
    }
}
