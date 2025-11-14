using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ChainMinigameThree : IMinigame
{
    [SerializeField] Transform area;
    [SerializeField] Transform pointer;
    [SerializeField] Transform successArc;
    [SerializeField] int phases = 3;
    int currentPhase = 0;
    List<bool> results;


    private void Update()
    {
        if (currentPhase == 0)
            return;

        if (currentPhase == 1)
        {
            timeElapsed += Time.deltaTime;

            if (timeElapsed > timeTotal)
            {
                results.Add(false);
                NextPhase();
            }
        }
        else if (currentPhase == 2)
        {
            timeElapsed -= Time.deltaTime;

            if (timeElapsed < 0)
            {
                results.Add(false);
                NextPhase();
            }
        }
        else if (currentPhase == 3)
        {
            timeElapsed += Time.deltaTime;

            if (timeElapsed < 0)
            {
                results.Add(false);
                EndMinigame();
            }
        }


        RotatePointer();
    }

    void RotatePointer()
    {
        if(currentPhase == 0)
            return;
        
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
        if (currentPhase == 1)
        {
            if (timeElapsed >= successStart && timeElapsed <= successEnd)
            {
                results.Add(true);
                NextPhase();
            }
            else
            {
                results.Add(false);
                NextPhase();
            }
        }
        else if (currentPhase == 2)
        {
            if (timeElapsed >= successStart && timeElapsed <= successEnd)
            {
                results.Add(true);
                NextPhase();
            }
            else
            {
                results.Add(false);
                NextPhase();
            }
        }
        else if(currentPhase == 3)
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
    }

    private void NextPhase()
    {
        currentPhase += 1;
        float oldSuccessStart = successStart;
        successStart = timeTotal - successEnd;
        successEnd = timeTotal - oldSuccessStart;
        SetSuccessArc();
    }

    public override void StartMinigame(float timeTotal, float successStart, float successEnd)
    {
        base.StartMinigame(timeTotal, successStart, successEnd);
        results = new List<bool>();
        timeElapsed = 0;
        currentPhase = 1;
        RotatePointer();
        SetSuccessArc();
    }

    void SetSuccessArc()
    {
        float angle = 90f - (((successStart + successEnd) / 2) / timeTotal) * 180f;

        successArc.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}