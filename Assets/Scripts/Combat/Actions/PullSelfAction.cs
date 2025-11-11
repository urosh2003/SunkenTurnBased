using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PullSelfAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    public PullSelfAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.APcost = 2;
        this.range = 3;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.GetGameObjectAtTile(context.targetedTile) == null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            )
        {
            await CalculateCooldown();
            this.actor.ChangeAP(-this.APcost);
            Vector3 newCharacterPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(actorPosition, context.targetedTile, GridEntityType.CHARACTER);
            this.actor.MoveCharacter(newCharacterPosition);
            return true;
        }
        return false;
    }

    private async Task<int> CalculateCooldown()
    {
        int damage = actor.basicAttackDamage;
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameTwo();
            if (results[0])
            {
                damage += 1;
            }
            if(results[1])
            {
                damage += 1;
            }
        }
        return damage;
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range)
        {
            SelectedTilesManager.instance.DrawRedXTargetingEmpty(this.context.targetedTile);
        }
        else
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawCircleRange(actorPosition, this.range);
    }
}