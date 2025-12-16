using System;
using UnityEngine;

public class LongerChainsPerk : Perk
{
    public LongerChainsPerk(Character character, int rangeGain) : base(character)
    {
        character.basicAttackRange += rangeGain;
    }

    public override void Initialize()
    {
        owner.OnCharacterActionInitiated += CheckForSkillshot;

    }

    private void CheckForSkillshot(IAction action)
    {
        if (action.isSkillshot)
        {
            action.range += 50;
        }
    }

    public override void OnRemove()
    {
        owner.OnCharacterActionInitiated -= CheckForSkillshot;
    }
}