using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Actions/Action Data")]
public class ActionData : ScriptableObject
{
    public string actionName;
    public string actionDescription;
    public int baseAPCost;
    public int baseCooldown;
    public int baseRange;
    public Sprite uiSprite;
}