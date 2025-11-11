using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WhirlpoolAction : IAction
{
    Vector3Int actorPosition;
    public WhirlpoolAction(Character actor) : base(actor)
    {
        this.actor = actor;
        actorPosition = GridEntitiesManager.instance.GetCellFromPosition(actor.transform.position);
        this.APcost = 2;
        this.range = 1;
    }

    public async override Task<bool> Execute()
    {
        if (this.context.targetedTile != null &&
            this.actor.currentAP >= this.APcost &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            )
        {
            List<Character> targets = GridEntitiesManager.instance.GetAdjacentGameObjects(actorPosition);
            if (targets.Count != 0)
            {
                List<int> damage = await CalculateDamage(targets);
                for (int i = 0; i < targets.Count; i++)
                {
                    targets[i].TakeDamage(damage[i]);
                }
            }
            this.actor.ChangeAP(-this.APcost);
            return true;
        }
        return false;
    }

    private async Task<List<int>> CalculateDamage(List<Character> targets)
    {
        List<int> damage = new();
        if (actor is PlayerCharacter)
        {
            List<bool> results = await MinigameManager.instance.PlayMinigameFour(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                damage.Add(1);

                if (results[i])
                    damage[i] += 1;
            }
        }
        return damage;
    }

    public override void RedrawTiles()
    { 
        if (this.context.targetedTile != null &&
            this.context.targetedTile != actorPosition &&
            GridEntitiesManager.instance.DistanceToTile(actorPosition, this.context.targetedTile) <= this.range
            )
        {
            SelectedTilesManager.instance.DrawCircleRedXTargeting(actorPosition, this.range);
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