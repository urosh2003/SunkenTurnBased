using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PullEnemyAction : IAction
{
    Vector3Int actorPosition;
    int direction;
    public PullEnemyAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.APcost = 2;
        this.range = 3;
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
            Character target = GridEntitiesManager.instance.GetFirstCharacterInDirection(actorPosition, range, direction);
            if (target)
            {
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.FocusOnPairAsync(actor.transform, target.transform);
                }
                int damage = await CalculateDamage();
                if(target && target.currentHealth > 0)
                {
                    Vector3 newCharacterPosition = GridEntitiesManager.instance.HookCharacter(actorPosition, range, direction);
                    target.MoveCharacter(newCharacterPosition, false);
                }
                if (actor is PlayerCharacter)
                {
                    await CameraActionFocus.instance.MinigameDone();
                }
            }
            this.actor.ChangeAP(-this.APcost);
            return true;
        }
        return false;
    }

    private async Task<int> CalculateDamage()
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
                damage += 1;
            }
        }
        return damage;
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