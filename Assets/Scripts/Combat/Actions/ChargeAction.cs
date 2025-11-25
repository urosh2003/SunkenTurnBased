using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ChargeAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    public ChargeAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.range = 4;
        this.baseAPcost = 2;
        this.APcost = this.baseAPcost + actor.GetCostModifiers(this);
        this.cooldown = 3;
    }

    public override bool UpdateContext(ActionContext newContext)
    {
        if (this.context.Equals(newContext) || resolving)
        {
            return false;
        }

        this.context = newContext;
        direction = GridEntitiesManager.instance.GetDirection(actorPosition, range, context.targetedTile);

        return true;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            direction != -1 &&
            !resolving
            )
        {
            resolving = true;
            Vector3Int targetTile = GridEntitiesManager.instance.GetTileInDirection(actorPosition, this.range, direction);
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, GridEntitiesManager.instance.GetCellCenter(targetTile));
            }
            int damage = await CalculateCooldown();
            bool done = false;
            for (int i = 0; i < this.range; i++)
            {
                Vector3Int nextTile =
                    GridEntitiesManager.instance.GetTileInDirection(actorPosition, 1, direction);
                Character target = GridEntitiesManager.instance.GetCharacterAtTile(nextTile);
                if (target)
                {
                    Vector3 newTargetPosition = GridEntitiesManager.instance.MoveCharacterInDirection(nextTile, direction);
                    target.MoveCharacter(newTargetPosition, false);
                    actor.CharacterMovedSomeone(target, newTargetPosition);
                    target.TakeDamage(damage);
                    if(target == null || target.currentHealth <= 0)
                    {
                        actor.CharacterKilledEnemy();
                    }
                    done = true;
                }

                Vector3 newActorPosition = GridEntitiesManager.instance.MoveCharacterInDirection(actorPosition, direction);
                actor.MoveCharacter(newActorPosition, false);
                actorPosition = GridEntitiesManager.instance.GetCellFromPosition(newActorPosition);

                if (done)
                {
                    break;
                }
            }
            if (actor is PlayerCharacter)
            {
                await CameraActionFocus.instance.MinigameDone();
            }
            this.actor.ChangeAP(-this.APcost);
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
        }
        return damage + bonusDamage;
    }

    public override void RedrawTiles()
    {
        if (this.context.targetedTile != null && !resolving)
        {
            SelectedTilesManager.instance.DrawOneDirection(actorPosition, range, direction, new TileStyle(TileColor.RED, TileType.XTILE, TileLayer.TARGETING));
        }
        else if(!resolving)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
        }
    }

    public override void DrawTiles()
    {
        SelectedTilesManager.instance.DrawAllDirections(actorPosition, this.range, new TileStyle(TileColor.RED, TileType.DEFAULT, TileLayer.RANGE));
    }
}