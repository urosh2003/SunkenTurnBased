using System.Collections.Generic;
using UnityEngine;

public class ChainMinigameFour : IMinigame
{
    [SerializeField] Transform pointer;
    [SerializeField] Transform successArcPrefab;  // Prefab of the success arc image
    [SerializeField] Transform area;              // Parent container for arcs
    [SerializeField, Range(1, 6)] int successZoneCount = 3;

    private List<bool> results;
    private List<SuccessZone> successZones = new List<SuccessZone>();
    private List<bool> usedZones = new List<bool>();
    private List<Transform> activeArcs = new List<Transform>();

    [SerializeField] private float zoneDurationFraction = 0.1f; // Each success zone covers 10% of total time

    public void SetSize(int size)
    {
        successZoneCount = size;
    }

    private struct SuccessZone
    {
        public float startTime;
        public float endTime;
        public bool used;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > timeTotal)
        {
            while (results.Count < successZoneCount)
            {
                results.Add(false);
            }
            EndMinigame();
            return;
        }
        if (results.Count == successZoneCount)
        {
            EndMinigame();
            return;
        }
        RotatePointer();
    }

    void RotatePointer()
    {
        // Map elapsed time to full circle (starting from 90° going clockwise)
        float angle = 90f - (timeElapsed / timeTotal) * 360f;
        pointer.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public override void StartMinigame(float timeTotal, float successStart, float successEnd)
    {
        base.StartMinigame(timeTotal, successStart, successEnd);

        results = new List<bool>();
        usedZones = new List<bool>();
        for (int i = 0; i < successZoneCount; i++)
        {
            usedZones.Add(false);
        }
        timeElapsed = 0f;

        ClearOldArcs();
        GenerateSuccessZones();
    }

    public override void Hit()
    {
        float currentTime = timeElapsed;

        bool success = false;

        for (int i = 0; i < successZones.Count; i++)
        {
            var zone = successZones[i];

            if (currentTime >= zone.startTime && currentTime <= zone.endTime && !usedZones[i])
            {
                success = true;
                usedZones[i] = true;
            }
        }
        Debug.Log(success);
        results.Add(success);
    }

    public override void EndMinigame()
    {
        MinigameManager.instance.EndMinigame(results);
    }

    void GenerateSuccessZones()
    {
        successZones.Clear();

        // Evenly spaced over the total time
        float baseTime = timeTotal / (successZoneCount+1);
        float zoneLength = timeTotal * zoneDurationFraction; // e.g. 10% of total duration
        float randomOffset = baseTime * 0.2f;

        for (int i = 1; i < successZoneCount+1; i++)
        {
            float nominalCenter = (i + 0.5f) * baseTime;
            float offset = Random.Range(-randomOffset, randomOffset);
            float centerTime = nominalCenter + offset;

            float start = Mathf.Clamp(centerTime - zoneLength / 2f, 0, timeTotal);
            float end = Mathf.Clamp(centerTime + zoneLength / 2f, 0, timeTotal);

            SuccessZone zone = new SuccessZone
            {
                startTime = start,
                endTime = end,
                used = false
            };

            successZones.Add(zone);
            CreateArcVisual(zone);
        }
    }

    void CreateArcVisual(SuccessZone zone)
    {
        var arc = Instantiate(successArcPrefab, area);
        float angle = 90f - (((zone.startTime + zone.endTime) / 2) / timeTotal) * 360f;

        arc.rotation = Quaternion.Euler(0f, 0f, angle);
        activeArcs.Add(arc);
    }

    void ClearOldArcs()
    {
        foreach (var a in activeArcs)
            Destroy(a.gameObject);
        activeArcs.Clear();
    }
}
