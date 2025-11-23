using System;

public class PullPerk : Perk
{
    bool nextPullSelfIsFree;
    bool nextPullEnemyIsFree;

    public PullPerk(Character character, Character relatedCharacter = null) : base(character, relatedCharacter)
    {
        nextPullSelfIsFree = false;
        nextPullEnemyIsFree = false;
    }

    public override void Initialize()
    {
        owner.OnCharacterActed += CheckForDiscountUpdate;
        owner.OnCharacterActionInitiated += CheckForDiscount;
    }

    private void CheckForDiscountUpdate(IAction action)
    {
        if (action is PullEnemyAction)
        {
            nextPullSelfIsFree = true;
        }
        else if (action is PullSelfAction)
        {
            nextPullEnemyIsFree = true;
        }
    }

    private void CheckForDiscount(IAction action)
    {
        if (action is PullEnemyAction && nextPullEnemyIsFree)
        {
            action.APcost = 0;
            nextPullEnemyIsFree = false;
        }
        else if (action is PullSelfAction && nextPullSelfIsFree)
        {
            action.APcost = 0;
            nextPullSelfIsFree = false;
        }
    }

    public override void OnRemove()
    {
        throw new System.NotImplementedException();
    }
}