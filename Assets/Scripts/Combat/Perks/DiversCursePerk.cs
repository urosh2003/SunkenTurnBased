using UnityEngine;

public class DiversCursePerk : Perk
{
    int damage;

    public DiversCursePerk(Character character, int damage) : base(character)
    {
        this.damage = damage;
    }

    public override void Initialize()
    {
        owner.OnCharacterMovedSomeone += SetCurse;
    }

    private void SetCurse(Character target, Vector3 newPosition)
    {
        target.AddStatusEffect(new Cursed(target, owner, damage));
    }

    public override void OnRemove()
    {
        owner.OnCharacterMovedSomeone -= SetCurse;
    }
}