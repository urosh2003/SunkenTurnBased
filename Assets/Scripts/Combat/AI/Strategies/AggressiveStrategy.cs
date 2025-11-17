using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AggressiveStrategy : IStrategy
{
    public override async Task ExecuteTurn(NpcAI npcAI)
    {
        if (npcAI.gameObject.GetComponent<Character>().currentAP > 0 &&
            npcAI.gameObject.GetComponent<Character>().canMove &&
            GridEntitiesManager.instance.DistanceToTileWorld(npcAI.gameObject.transform.position,
        PlayerManager.instance.transform.position) > npcAI.gameObject.GetComponent<Character>().basicAttackRange)
        {
            ActionContext actionContext = new ActionContext();
            actionContext.targetedCharacter = PlayerManager.instance.playerCharacter;
            MoveToCharacterAction moveAction = new MoveToCharacterAction(npcAI.GetComponent<Character>());
            moveAction.UpdateContext(actionContext);
            await moveAction.Execute();
        }
        else if (npcAI.gameObject.GetComponent<Character>().currentAP >= 2 &&
                GridEntitiesManager.instance.DistanceToTileWorld(npcAI.gameObject.transform.position,
                PlayerManager.instance.transform.position) <= npcAI.gameObject.GetComponent<Character>().basicAttackRange)
        {
            await Act(npcAI);
            npcAI.ThinkAndAct();
        }
        else
        {
            npcAI.EndTurn();
        }
    }

    public override async Task Act(NpcAI npcAI)
    {
        BasicAttackAction attackAction = new BasicAttackAction(npcAI.gameObject.GetComponent<Character>());
        ActionContext actionContext = new ActionContext();
        actionContext.targetedTile = GridEntitiesManager.instance.GetCellFromPosition(PlayerManager.instance.transform.position);
        attackAction.UpdateContext(actionContext);
        await attackAction.Execute();
    }

}