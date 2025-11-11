using System;
using UnityEngine;

public abstract class IMinigame: MonoBehaviour
{
    public float timeTotal;
    public float successStart;
    public float successEnd;
    public float timeElapsed = 0;

    public virtual void StartMinigame(float timeTotal, float successStart, float successEnd)
    {
        this.timeTotal = timeTotal;
        this.successStart = successStart;
        this.successEnd = successEnd;
    }

    public abstract void Hit();

    public abstract void EndMinigame();
}