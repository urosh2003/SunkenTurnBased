using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NpcAI : MonoBehaviour
{
    [SerializeField] private INpcState currentState;
    [SerializeField] private IStrategy currentStrategy;
    public bool moving;
    public float waitForMove = 0.2f;
    public float timeElapsed = 0f;
    public List<Vector3Int> movementPath;

    public int movementElapsed = 0;


    internal void ExecuteTurn()
    {     
        currentStrategy.ExecuteTurn(this);       
    }

    private void Start()
    {
        currentStrategy = new AggressiveStrategy();
    }

    public void ActionDone()
    {
        ExecuteTurn();
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void EndTurn()
    {
        this.gameObject.GetComponent<Character>().EndTurn();
    }

    public void DrawMovement()
    {
        if (movementPath != null)
            ColorPath(movementPath);
    }

    private void ColorPath(List<Vector3Int> path)
    {
        
    }


    public float moveDuration = 0.3f; // Time to reach target
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Default ease

    public void MoveNpc(Vector3 targetPosition)
    {
        moving = true;
        StartCoroutine(MoveCoroutine(targetPosition, moveDuration));
    }

    private IEnumerator MoveCoroutine(Vector3 target, float duration)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            //t = easeCurve.Evaluate(t); // Apply easing from the curve
            transform.position = Vector3.Lerp(startPos, target, t);
            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        transform.position = target; // Ensure exact position

        if(TurnManager.instance.currentTurn == GetComponent<Character>())
            this.ActionDone();
    }

    private float thinkTime = 1.5f;

    public void ThinkAndAct()
    {
        StartCoroutine(ThinkAndActCoroutine());
    }


    private IEnumerator ThinkAndActCoroutine()
    {
        yield return new WaitForSeconds(thinkTime);

        Act();
    }

    private void Act()
    {
        this.currentStrategy.Act(this);
        this.ActionDone();
    }
}
