using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    internal void StartTurn()
    {
        ThinkAndAct();       
    }

    private void Start()
    {
        currentStrategy = new AggressiveStrategy();
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void EndTurn()
    {
        this.gameObject.GetComponent<Character>().EndTurn();
    }

    public float moveDuration = 0.3f; // Time to reach target
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Default ease

    public void MoveNpc(Vector3 targetPosition, bool wholeAction, int tilesMoved)
    {
        moving = true;
        StartCoroutine(MoveCoroutine(targetPosition, moveDuration, wholeAction, tilesMoved));
    }

    private IEnumerator MoveCoroutine(Vector3 target, float duration, bool wholeAction, int tilesMoved)
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
        this.gameObject.GetComponent<Character>().CharacterMoved(tilesMoved);
        if (wholeAction)
        {
            ThinkAndAct();
        }
    }

    [SerializeField] private float thinkTime = 0.5f;

    public void ThinkAndAct()
    {
        StartCoroutine(ThinkAndActCoroutine());
    }


    private IEnumerator ThinkAndActCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < thinkTime)
        {
            elapsed += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        _ = ActAsync();
    }

    async Task ActAsync()
    {
        if (!this.gameObject.GetComponent<Character>().canAct)
        {
            EndTurn();
            return;
        }

        await currentStrategy.ExecuteTurn(this);
    }
}
