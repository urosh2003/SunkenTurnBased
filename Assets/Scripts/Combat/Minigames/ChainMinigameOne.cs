using System.Collections.Generic;
using UnityEngine;

public class ChainMinigameOne : IMinigame
{
    [SerializeField] Transform area;
    [SerializeField] Transform pointer;
    [SerializeField] Transform successArc;
    List<bool> results;

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > timeTotal)
        {
            results.Add(false);
            EndMinigame();
        }

        RotatePointer();
    }

    void RotatePointer()
    {
        // Map time to angle (starts at 90° and goes to -90° over the duration)
        float angle = 90f - (timeElapsed / timeTotal) * 180f;

        pointer.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public override void EndMinigame()
    {
        MinigameManager.instance.EndMinigame(results);
    }

    public override void Hit()
    {
        if (timeElapsed >= successStart && timeElapsed <= successEnd)
        {
            results.Add(true);
            EndMinigame();
        }
        else
        {
            results.Add(false);
            EndMinigame();
        }
    }

    public override void StartMinigame(float timeTotal, float successStart, float successEnd)
    {
        base.StartMinigame(timeTotal, successStart, successEnd);
        results = new List<bool>();
        timeElapsed = 0;
        RotatePointer();
        SetSuccessArc();
    }

    void SetSuccessArc()
    {
        float angle = 90f - (((successStart + successEnd) / 2) / timeTotal) * 180f;

        successArc.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}