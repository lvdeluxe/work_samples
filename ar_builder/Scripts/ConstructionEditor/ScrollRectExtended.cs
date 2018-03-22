using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollRectExtended : ScrollRect, IPointerExitHandler {

	public bool hasExited = false;

	private bool _dragging;

	public void OnPointerExit (PointerEventData eventData)
	{
		if(_dragging)
			hasExited = true;
	}

	override public void OnBeginDrag(PointerEventData eventData){
		hasExited = false;
		base.OnBeginDrag(eventData);
		_dragging = true;
	}

	override public void OnEndDrag(PointerEventData eventData){
		_dragging = false;
		hasExited = false;
		base.OnEndDrag(eventData);
	}
}
