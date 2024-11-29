using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTipControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(true);
    }

    public void CloseTooltip()
    {
        this.gameObject.SetActive(false);
    }
}
