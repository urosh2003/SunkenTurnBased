using System.Linq;
using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    public GameObject button;

    private void Awake()
    {
        button = Resources.FindObjectsOfTypeAll<GameObject>()
        .FirstOrDefault(obj => obj.name == "EndTurnButton");
        DisableButton();
        TurnManager.combatStart += EnableButton;
        TurnManager.combatEnd += DisableButton;
    }

    private void EnableButton()
    {
        button.SetActive(true);
    }

    private void DisableButton()
    {
        button.SetActive(false);
    }
}