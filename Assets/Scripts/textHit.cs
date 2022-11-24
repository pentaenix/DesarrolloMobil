using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class textHit : MonoBehaviour
{
    // Start is called before the first frame update
    string goodText = "+3";
    TMP_Text text;
    string negativeText;
    public bool CanUse = true;
    void Start()
    {
        text = GetComponent<TMP_Text>();
        negativeText = text.text;
    }

    public void Activate(Transform position, bool isPositive) {
        CanUse = false;
        text.rectTransform.position = position.position;
        text.text = (isPositive ? goodText : negativeText);
        text.DOColor(isPositive ? Color.green : Color.red, 0);
        text.DOFade(0, 0.5f);
        text.rectTransform.DOMoveY(text.rectTransform.position.y + 2, 1f);
    }

    public void resetText() {
        text.rectTransform.position = new Vector2(500, 500);
    }

    // Update is called once per frame
    void Update()
    {
        if(text.color.a < 0.1f) {
            CanUse = true;
		}
    }
}
