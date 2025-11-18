using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class IAction
{
    public int APcost;

    public Character actor;

    public ActionContext context;

    public int range;
    public bool resolving = false;
    public int cooldown;

    public abstract Task<bool> Execute();

    public virtual bool UpdateContext(ActionContext newContext)
    {
        if (this.context.Equals(newContext) || resolving)
        {
            return false;
        }
        else
        {
            this.context = newContext;
            return true;
        }
    }

    public IAction(Character actor)
    {
        this.actor = actor;
    }

    public abstract void RedrawTiles();

    public abstract void DrawTiles();
}
