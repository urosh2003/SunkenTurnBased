using System;
using UnityEngine;

public class KillRushPerk : Perk
{
    int APGain;
    public KillRushPerk(Character character, int APGain) : base(character)
    {
        this.APGain = APGain;
    }

    public override void Initialize()
    {
        owner.OnCharacterKilledEnemy += RefreshAP;

    }

    private void RefreshAP()
    {
        owner.ChangeAP(+APGain);
    }

    public override void OnRemove()
    {
        owner.OnCharacterKilledEnemy -= RefreshAP;
    }
}