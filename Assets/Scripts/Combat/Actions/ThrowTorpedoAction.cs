using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ThrowTorpedoAction : IAction
{
    Vector3Int actorPosition;
    Vector3Int targetPosition;
    Character target;
    int phase = 0;
    int minRange;
    public ThrowTorpedoAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = 3;
        this.minRange = 2;
        this.phase = 1;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 3;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) >= this.minRange &&
            this.phase == 1 &&
            !resolving
            )
        {
            phase = 2;
            targetPosition = context.targetedTile;
            target = GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile);
            DrawTiles();

            return false;
        }
        else if (this.phase == 2 && 
            this.context.targetedTile != null &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.GetCharacterAtTile(context.targetedTile) == null &&
            GridEntitiesManager.instance.DistanceToTile(targetPosition, this.context.targetedTile) <= 1 &&
            !resolving
            )
        {
            resolving = true;
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, GridEntitiesManager.instance.GetCellCenter(context.targetedTile));
            }
            int damage = await CalculateCooldown();
            target.TakeDamage(damage);
            if(target)
            {
                Vector3 newTargetPosition = GridEntitiesManager.instance.MoveEntityToTilePosition(targetPosition, context.targetedTile, GridEntityType.CHARACTER);
                target.MoveCharacter(newTargetPosition, false);
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
            if (results[1])
            {
                cooldown -= 1;
            }
            if (results[2])
            {
                cooldown -= 1;
            }
        }
        return damage;
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range && !resolving && this.phase == 1 &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) >= this.minRange)
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (this.phase == 2 &&
            GridEntitiesManager.instance.DistanceToTile(targetPosition, this.context.targetedTile) <= 1)
        {
            SelectedTilesManager.instance.DrawSingle(this.context.targetedTile, new TileStyle(TileColor.YELLOW, TileType.XTILE, TileLayer.TARGETING));
        }
        else if (!resolving && this.phase == 2)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
        else if (!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        if (phase == 1)
        {
            SelectedTilesManager.instance.DrawCircle(actorPosition, this.range, new TileStyle(TileColor.RED, TileType.DEFAULT, TileLayer.RANGE));
            SelectedTilesManager.instance.DeleteCircle(actorPosition, this.range - this.minRange, new TileStyle(TileColor.RED, TileType.DEFAULT, TileLayer.RANGE));
        }
        else 
        {
            SelectedTilesManager.instance.DrawCircle(targetPosition, 1, new TileStyle(TileColor.YELLOW, TileType.DEFAULT, TileLayer.RANGE));
            SelectedTilesManager.instance.DrawSingle(targetPosition, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.RANGE));
        }
    }
}