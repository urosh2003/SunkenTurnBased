using System;
using UnityEngine;

public class QuickPunchPerk : Perk
{
    bool discountAvailable;
    bool resetBuff;
    public QuickPunchPerk(Character character) : base(character)
    {
        discountAvailable = false;
        resetBuff = true;
    }

    public override void Initialize()
    {
        owner.OnCharacterMovedSomeone += CheckForDiscountUpdate;
        owner.OnCharacterActionInitiated += CheckForDiscount;
        owner.OnCharacterActed += CheckForResetBuff;
    }

    private void CheckForResetBuff(IAction action)
    {
        if (resetBuff)
        {
            discountAvailable = false;
        }
        else
        {
            resetBuff = true;
        }
    }

    private void CheckForDiscount(IAction action)
    {
        if (action is BasicAttackAction && discountAvailable)
        {
            action.APcost = 0;
            discountAvailable = false;
        }
    }

    private void CheckForDiscountUpdate(Character character, Vector3 newPosition)
    {
        if(GridEntitiesManager.instance.DistanceToTileWorld(owner.transform.position, newPosition)
            <= owner.basicAttackRange)
        {
            discountAvailable = true;
            resetBuff = false;
        }
    }

    public override void OnRemove()
    {
        owner.OnCharacterMovedSomeone -= CheckForDiscountUpdate;
        owner.OnCharacterActionInitiated -= CheckForDiscount;
    }
}