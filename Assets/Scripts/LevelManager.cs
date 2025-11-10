using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public List<GameObject> levelObjects;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        TurnManager.instance.StartCombat(levelObjects);
    }
}