using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class TargetingState : IState
{

    public TargetingState(IAction action)
    {
        this.selectedAction = action;
    }

    public override async Task<bool> Execute()
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        bool finished = await selectedAction.Execute();
        return finished;
    }

    public override void Update(Vector3 mouseWorldPosition)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
            return;
        }

        Vector3Int targetedTile = GridEntitiesManager.instance.GetCellFromPosition(mouseWorldPosition);

        ActionContext newContext = new ActionContext();
        newContext.targetedTile = targetedTile;

        bool contextUpdated = selectedAction.UpdateContext(newContext);
        if (contextUpdated)
        {
            SelectedTilesManager.instance.ClearTargetingTiles();
            selectedAction.RedrawTiles();
            SelectedTilesManager.instance.DrawSingle(GridEntitiesManager.instance.GetCellFromPosition(PlayerManager.instance.playerCharacter.transform.position)
                , new TileStyle(TileColor.GREEN, TileType.DEFAULT, TileLayer.TARGETING));
        }
    }

    public override void Enter()
    {
        PlayerManager.instance.playerCharacter.CharacterActionInitiated(selectedAction);
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
