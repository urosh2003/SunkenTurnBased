
public enum PerkName { }

public abstract class Perk
{
    public PerkName name;

    public Character owner;
    public Character relatedCharacter;

    public Perk(Character character)
    {
        this.owner = character;
        Initialize();
    }

    public abstract void Initialize();

    public abstract void OnRemove();

    public void Remove()
    {
        OnRemove();
        owner.activePerks.Remove(this);
    }

}