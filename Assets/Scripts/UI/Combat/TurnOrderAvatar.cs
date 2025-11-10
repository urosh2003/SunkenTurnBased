using UnityEngine;
using UnityEngine.UI;

public class TurnOrderAvatar : MonoBehaviour
{
    public Image portrait;
    public Character character;
    public GameObject highlight;

    public void Initialize(Character character, Sprite defaultSprite)
    {
        this.character = character;
        portrait.sprite = character.avatar != null ? character.avatar : defaultSprite;
    }

    public void SetHighlight(bool isActive)
    {
        highlight.SetActive(isActive);
    }

    public void EnableHighlight()
    {
        this.character.GetComponent<Highlight>().EnableHighlight();
    }

    public void DisableHighlight()
    {
        this.character.GetComponent<Highlight>().DisableHighlight();
    }
}