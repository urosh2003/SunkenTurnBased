using System;
using System.Collections.Generic;
using UnityEngine;

public class SkilledPerk : Perk
{
    public SkilledPerk(Character character) : base(character)
    {
    }

    public override void Initialize()
    {
        MinigameManager.minigameDone += CheckForAce;

    }

    private void CheckForAce(List<bool> minigameResults)
    {
        foreach (bool result in minigameResults)
        {
            if (!result)
            {
                return;
            }
        }
        PlayerManager.instance.currentState.selectedAction.cooldown -= 1;
        PlayerManager.instance.currentState.selectedAction.minigameBonusDamage *= 2;
    }

    public override void OnRemove()
    {
        MinigameManager.minigameDone -= CheckForAce;

    }
}