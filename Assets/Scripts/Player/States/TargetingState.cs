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
        if (selectedAction.resolving)
        {
            return;
        }
        

        Vector3Int targetedTile = GridEntitiesManager.instance.GetCellFromPosition(mouseWorldPosition);
        ActionContext newContext = new ActionContext();
        newContext.targetedTile = targetedTile;

        if (!selectedAction.resolving)
        {
            bool contextUpdated = selectedAction.UpdateContext(newContext);
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
        SelectedTilesManager.instance.UnLockHighlights();
        SelectedTilesManager.instance.ClearRangeTiles();
        SelectedTilesManager.instance.ClearTargetingTiles();
    }
}
