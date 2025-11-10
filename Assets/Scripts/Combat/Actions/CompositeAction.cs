using System.Collections.Generic;
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

    public override bool Execute()
    {
        foreach (var action in actions)
        {
            action.Execute();
        }
        return true;
    }

    public override void RedrawTiles()
    {
        throw new System.NotImplementedException();
    }
}