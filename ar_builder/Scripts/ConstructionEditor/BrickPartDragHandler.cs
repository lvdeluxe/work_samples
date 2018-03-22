using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrickPartDragHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerExitHandler {

	public ScrollRectExtended scrollRect;
	public ConstructionEditorUI editor;

	private bool _isDragging = false;
	private bool _dragStarted = false;

	[HideInInspector]
	public bool reversedPart = false;
	[HideInInspector]
	public int designId;

	void Start(){

	}

	public void OnPointerExit (PointerEventData eventData)
	{
		if(_isDragging && !_dragStarted  && scrollRect.hasExited){
			editor.OnDragStartPart(designId, reversedPart, eventData.position);
			_dragStarted = true;
		}
	}


	public void OnDrag (PointerEventData eventData)
	{
		if(!scrollRect.hasExited)
			scrollRect.OnDrag(eventData);
		if(scrollRect.hasExited && !_dragStarted)
			OnPointerExit(eventData);
	}

	public void OnEndDrag (PointerEventData eventData)
	{
		scrollRect.OnEndDrag(eventData);
		editor.OnDragEndPart(eventData.selectedObject);
		_dragStarted = false;
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		_isDragging = true;
		scrollRect.OnBeginDrag(eventData);

	}

	public void OnPointerClick (PointerEventData eventData)
	{
		if(!_isDragging)
			editor.OnClickPart(designId, reversedPart);
		_isDragging = false;
	}


}
