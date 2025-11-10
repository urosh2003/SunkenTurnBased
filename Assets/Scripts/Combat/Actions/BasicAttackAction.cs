using System.Collections.Generic;
using UnityEngine;

public class BasicAttackAction : IAction
{
    Vector3Int actorPosition;
    public BasicAttackAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.APcost = 2;
        this.range = actor.basicAttackRange;
    }

    public override bool Execute()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            && this.actor.currentAP >= this.APcost &&
            GridEntitiesManager.instance.GetGameObjectAtTile(context.targetedTile) != actor
            )
        {
            Character target = GridEntitiesManager.instance.GetGameObjectAtTile(context.targetedTile);
            if (target != null)
            {
                target.TakeDamage(this.actor.basicAttackDamage);
            }
            this.actor.ChangeAP(-this.APcost);
            return true;
        }
        return false;
    }

    public override void RedrawTiles()
    { 
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            GridEntitiesManager.instance.GetGameObjectAtTile(context.targetedTile) != actor
            )
        {
            SelectedTilesManager.instance.DrawRedXTargeting(this.context.targetedTile);
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