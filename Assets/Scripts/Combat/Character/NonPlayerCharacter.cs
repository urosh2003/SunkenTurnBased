using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerCharacter : Character
{
    public override void StartTurn()
    {
        Debug.Log("NPC");
        base.StartTurn();
        this.gameObject.GetComponent<Highlight>().EnableHighlight();
        this.gameObject.GetComponent<NpcAI>().ExecuteTurn();
    }
    public override void EndTurn()
    {
        this.gameObject.GetComponent<Highlight>().DisableHighlight();
        base.EndTurn();
    }

    public override void MoveCharacter(Vector3 target)
    {
        this.gameObject.GetComponent<NpcAI>().MoveNpc(target);
    }

    public override void TakeDamage(int value)
    {
        base.TakeDamage(value);
        if(currentHealth<=0)
        {
            GridEntitiesManager.instance.RemoveGridEntityWorld(this.transform.position, GridEntityType.CHARACTER);
            TurnManager.instance.RemoveCharacter(this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
