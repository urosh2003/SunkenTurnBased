using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompositeAction : IAction
{
    public List<IAction> actions;

    public CompositeAction(Character actor) : base(actor)
    {
    }

    public override void DrawTiles()
    {
        throw new System.NotImplementedException();
    }

    public override async Task<bool> Execute()
    {
        foreach (var action in actions)
        {
            await action.Execute();
        }
        return true;
    }

    public override void RedrawTiles()
    {
        throw new System.NotImplementedException();
    }
}