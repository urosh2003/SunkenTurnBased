using UnityEngine;

public class ActionHub : MonoBehaviour
{
    public Transform panel;
    public GameObject iconPrefab;
    public static ActionHub instance;
    private void Awake()
    {
        instance = this;
    }
    public ActionButton AddActionButton(int number, ActionData actionSprite)
    {
        GameObject newButton = Instantiate(iconPrefab, panel);
        return newButton.GetComponent<ActionButton>().SetUpActionButton(number, actionSprite);
    }
}