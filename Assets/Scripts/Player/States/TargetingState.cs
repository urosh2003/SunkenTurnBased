using System.Threading.Tasks;
using UnityEngine;

public class TargetingState : IState
{
    public IAction selectedAction;

    public TargetingState(IAction action)
    {
        this.selectedAction = action;
    }

    public override async Task<bool> Execute()
    {
        bool finished = await selectedAction.Execute();
        return finished;
    }

    public override void Update(Vector3 mouseWorldPosition)
    {
        Vector3Int targetedTile = GridEntitiesManager.instance.GetCellFromPosition(mouseWorldPosition);

        ActionContext newContext = new ActionContext();
        newContext.targetedTile = targetedTile;

        bool contextUpdated = selectedAction.UpdateContext(newContext);
        if (contextUpdated)
        {
            selectedAction.RedrawTiles();
        }
    }

    public override void Enter()
    {
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
        selectedAction.DrawTiles();
    }

    public override void Exit()
    {
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
    }
}
