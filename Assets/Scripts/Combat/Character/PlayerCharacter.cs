using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character
{
    public static event Action OnEndPlayerTurn;
    public static event Action OnStartPlayerTurn;
    public static event Action<int> OnPlayerStaminaChange;
    public static event Action<int> OnPlayerHealthChange;

    public override void StartTurn()
    {
        RefreshResources();
        OnStartPlayerTurn?.Invoke();
        //OnCharacterStartTurn?.Invoke();
        InvokeOnStartCharacterTurn();
        this.gameObject.GetComponent<Highlight>().EnableHighlight();
    }

    public override void EndTurn()
    {
        if (TurnManager.instance.currentTurn == this)
        {
            this.gameObject.GetComponent<Highlight>().DisableHighlight();
            //OnCharacterEndTurn?.Invoke();
            OnEndPlayerTurn?.Invoke();
        }
    }
    /*
    public override void MoveCharacter(Vector3 target)
    {
        GameStateManager.Instance.MovePlayer(target);
    }
    */

    public float moveDuration = 0.3f; // Time to reach target
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Default ease
    public static event Action animationDone;

    public override void MoveCharacter(Vector3 targetPosition, bool wholeAction)
    {
        StartCoroutine(MoveCoroutine(targetPosition, moveDuration, wholeAction));
    }

    private IEnumerator MoveCoroutine(Vector3 target, float duration, bool wholeAction)
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
        if (wholeAction)
        {
            PlayerCharacter.animationDone?.Invoke();
        }
    }

    public override void RefreshResources()
    {
        base.RefreshResources();
        OnPlayerStaminaChange?.Invoke(currentAP);
    }

    public override void ChangeAP(int value)
    {
        base.ChangeAP(value);
        OnPlayerStaminaChange?.Invoke(currentAP);
    }

    public override void TakeDamage(int value)
    {
        base.TakeDamage(value);
        OnPlayerHealthChange?.Invoke(currentHealth);
    }
}
