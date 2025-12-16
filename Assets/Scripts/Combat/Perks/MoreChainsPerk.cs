using System;
using System.Collections.Generic;

public class MoreChainsPerk : Perk
{

    public MoreChainsPerk(Character character) : base(character)
    {
    }

    public override void Initialize()
    {
        owner.OnCharacterDamagedEnemy += RootEnemies;
    }

    private void RootEnemies(List<Character> characters, List<int> damage)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i] != null && damage[i] > 0)
            {
                characters[i].AddStatusEffect(new RootEffect(1, characters[i]));
            }
        }
    }

    public override void OnRemove()
    {
        owner.OnCharacterDamagedEnemy -= RootEnemies;

    }
}