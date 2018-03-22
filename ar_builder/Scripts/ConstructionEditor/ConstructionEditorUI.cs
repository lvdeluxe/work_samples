using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConstructionEditorUI : MonoBehaviour {

	public GameObject colorButtonPrefab;
	public GameObject partButtonPrefab;

	public Transform colorsListUI;
	public Transform partsListUI;

	public Button removeBrickButton;

	private ConstructionEditor _modelEditor;

	public Text warningLabel;

	void Start () {
		warningLabel.gameObject.SetActive(false);
		_modelEditor = GetComponent<ConstructionEditor>();
	}

	public void OnClickValid(){
		if(!warningLabel.gameObject.activeSelf){
			warningLabel.gameObject.SetActive(true);
			if(_modelEditor.TestAllFloatingBricks()){
				warningLabel.color = Color.green;
				warningLabel.text = "Everything is awesome!";
				_modelEditor.SaveConstruction();
			}else{
				warningLabel.color = Color.red;
				warningLabel.text = "Floating Bricks...Please fix!";
				StartCoroutine(DisableWarning());
				_modelEditor.Unselect();
			}
		}
	}

	IEnumerator DisableWarning(){
		yield return new WaitForSeconds(5.0f);
		warningLabel.gameObject.SetActive(false);
		_modelEditor.ClearAllFloatingBricks();
	}

	public void OnPressScreen(){
		StopAllCoroutines();
		warningLabel.gameObject.SetActive(false);
		_modelEditor.ClearAllFloatingBricks();
		_modelEditor.CheckSelectedBrick();
	}
	public void OnReleaseScreen(){
		_modelEditor.ReleaseDragPart();
	}

	public void UpdateColorsList(int[] colors, int selectedColor){
		foreach (Transform tr in colorsListUI) {
			tr.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
			Destroy (tr.gameObject);
		}

		for (int i = 0; i < colors.Length; i ++) {
			GameObject btn = Instantiate (colorButtonPrefab) as GameObject;
			Image img = btn.transform.GetChild(0).GetComponent<Image> ();
			img.color = LXFMLHelper.GetBrickColor(colors[i]);
			if (img.color.r + img.color.g + img.color.b < 0.1f)
				img.color += new Color (.1f, .1f, .1f, 1);
			btn.name = colors[i].ToString();
			btn.transform.SetParent(colorsListUI, false);
			int matId = colors[i]; 
			var button = btn.GetComponent<Button>();
			button.onClick.AddListener(()=>{_modelEditor.UpdateColorPart(matId);});
			ColorBlock colorBlock = button.colors;
			colorBlock.pressedColor = LXFMLHelper.GetBrickColor(colors[i]);

			if(colors[i] == selectedColor){
				button.interactable = false;
				colorBlock.disabledColor = LXFMLHelper.GetBrickColor(colors[i]);
			}
			button.colors = colorBlock;
		}
	}

	public void OnDragStartPart(int designId, bool reverse, Vector2 screenPos){
		_modelEditor.StartDragPart(designId, reverse, screenPos);
	}

	public void OnDragEndPart(GameObject clicked){
		_modelEditor.StopDragPart();
	}

	public void OnDragPart(){
//		Debug.Log ("Drag");
	}

	public void OnClickDestroy(){
		_modelEditor.DestroyPart();
	}

	public void OnClickPart(int designId, bool reverse){
		_modelEditor.ReplacePart(designId,reverse);			
	}

	public void UpdatePartsList(List<int> list, bool showRemove, int selectedDesignId){

		removeBrickButton.interactable = showRemove;

		foreach (Transform tr in partsListUI) {
			Destroy (tr.gameObject);
		}

		for (int i = 0; i < list.Count; i ++) {
			if(list[i] != selectedDesignId){
				GameObject btn = Instantiate (partButtonPrefab) as GameObject;
				Image img = btn.transform.GetChild(0).GetComponent<Image> ();		
				btn.name = list [i].ToString();
				img.sprite = Resources.Load<Sprite> ("BrickTextures/" + list [i].ToString ());
				BrickPartDragHandler dragHandler = btn.GetComponent<BrickPartDragHandler>();
				dragHandler.designId = list[i];
				dragHandler.reversedPart = false;
				btn.transform.SetParent (partsListUI, false);
			}

			if(LXFMLHelper.IsSlope(list[i]) || LXFMLHelper.IsCurved(list[i])){
				GameObject btn_reverse = Instantiate (partButtonPrefab) as GameObject;
				Image img_reverse = btn_reverse.transform.GetChild(0).GetComponent<Image> ();		
				btn_reverse.name = list [i].ToString() + "_reverse";
				img_reverse.sprite = Resources.Load<Sprite> ("BrickTextures/" + list [i].ToString ());
				img_reverse.transform.localScale = new Vector3 (-1, 1, 1);
				BrickPartDragHandler dragHandler_reverse = btn_reverse.GetComponent<BrickPartDragHandler>();
				dragHandler_reverse.reversedPart = true;
				dragHandler_reverse.designId = list[i];
				btn_reverse.transform.SetParent (partsListUI, false);
			}
		}

	}

}
