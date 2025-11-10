using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    private SpriteRenderer highlight;

    public void Awake()
    {
        highlight = this.gameObject.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(obj => obj.name == "Highlight");
        highlight.enabled = false;
    }

    public void EnableHighlight()
    {
        highlight.enabled = true;
    }

    public void DisableHighlight()
    {
        highlight.enabled = false;
    }
}
