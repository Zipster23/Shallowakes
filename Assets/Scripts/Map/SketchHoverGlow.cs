using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SketchHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    [SerializeField] private Shadow shadow;
    [SerializeField] private Color glowColor = new Color(1f, 0.9f, 0.5f, 1f);




    private void Start()
    {
        
        shadow.enabled = false;

    }




    public void OnPointerEnter(PointerEventData eventData)
    {
        
        shadow.enabled = true;
        shadow.effectColor = glowColor;

    }




    public void OnPointerExit(PointerEventData eventData)
    {
        
        shadow.enabled = false;

    }

}
