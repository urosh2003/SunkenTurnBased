
public enum PerkName { }

public abstract class Perk
{
    public PerkName name;

    public Character owner;
    public Character relatedCharacter;

    public Perk(Character character, Character relatedCharacter = null)
    {
        this.owner = character;
        this.relatedCharacter = relatedCharacter;
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