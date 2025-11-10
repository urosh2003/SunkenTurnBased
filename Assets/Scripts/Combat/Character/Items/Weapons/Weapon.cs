using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Items/Weapons", order = 1)]
public class Weapon : ScriptableObject
{
    public int range;

    public int damage;
}