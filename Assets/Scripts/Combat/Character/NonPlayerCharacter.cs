using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : Character
{
    public override void StartTurn()
    {
        Debug.Log("NPC");
        base.StartTurn();
        this.gameObject.GetComponent<CharacterDetails>().EnableHighlight();
        this.gameObject.GetComponent<NpcAI>().StartTurn();
    }
    public override void EndTurn()
    {
        this.gameObject.GetComponent<CharacterDetails>().DisableHighlight();
        base.EndTurn();
    }

    public override void MoveCharacter(Vector3 target, bool wholeAction, int tilesMoved)
    {
        this.gameObject.GetComponent<NpcAI>().MoveNpc(target, wholeAction, tilesMoved);
    }

    public override void TakeDamage(int value)
    {
        base.TakeDamage(value);
    }
}
