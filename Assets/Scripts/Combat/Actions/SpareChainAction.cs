using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SpareChainAction : IAction
{
    Vector3Int actorPosition;
    int duration;
    public SpareChainAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = actor.basicAttackRange;
        this.duration = 1;
        this.baseAPcost = 1;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 1;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            && this.actor.currentAP >= this.APcost &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != actor &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != null &&
            !resolving
            )
        {
            resolving = true;
            Character target = GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile);
            if (target != null)
            {
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, target.transform);
                }
                await CalculateCooldown();
                target.AddStatusEffect(new RootEffect(this.duration,target));
            }
            this.actor.ChangeAP(-this.APcost);
            this.actor.CharacterAttacked(new List<Character> { target });
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.MinigameDone();
            }
            return true;
        }
        return false;
    }

    private async Task CalculateCooldown()
    {
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameOne();
            if (results[0])
            {
                cooldown -= 1;
            }
        }
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != actor &&
            !resolving
            )
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.RED, TileType.DEFAULT, TileLayer.RANGE));
    }
}