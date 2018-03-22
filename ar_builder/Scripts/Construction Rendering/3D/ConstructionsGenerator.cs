using UnityEngine;
using System.Collections;

public class ConstructionsGenerator : MonoBehaviour {


	public TextAsset[] models;
	public float delay = 3f;

	private int _modelIndex = 0;

	public bool paused = true;
	private bool _prevPause = true;

	void Start () {
		GenerateModel();

	}

	IEnumerator DisplayModel(){
		yield return new WaitForSeconds(delay);
		DestroyPreviousModel();
		IncrementIndex();
		GenerateModel();
	}

	void IncrementIndex(){
		_modelIndex++;
		if(_modelIndex == models.Length)
			_modelIndex = 0;
	}

	void DestroyPreviousModel(){
		Transform t = transform.FindChild("model");
		Destroy(t.gameObject);
	}

	void GenerateModel(){
		Transform t = new GameObject("model").transform;
		t.SetParent(transform);
		t.localPosition = Vector3.zero;
		var constructionModel = t.GetOrAddComponent<ConstructionModel>();
		constructionModel._constructionData = models[_modelIndex];
		constructionModel.Init ();
		if(!paused)
			StartCoroutine("DisplayModel");
	}

	void UpdatePause(){
		_prevPause = paused;
		if(paused){
			StopCoroutine("DisplayModel");
		}
		else{
			DestroyPreviousModel();
			IncrementIndex();
			GenerateModel();
		}
	}
	
	// Update is called once per frame
	void Update () {
//		if(paused != _prevPause){
//			UpdatePause();
//		}
	}
}
