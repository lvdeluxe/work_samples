using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Assertions;

using Hibernum.Core;

public class ConstructionController : IndestructibleSingletonBehaviour<ConstructionController> {

	[HideInInspector]
	public ConstructionResourcesProvider resourcesProvider;

	private Dictionary<Transform, ConstructionBuilder> _builders;

	private string _currentConstructionData = "";

	private int _numUserConstructions = 0;

	override protected void OnSingletonAwake () {
		base.OnSingletonAwake();
		_builders = new Dictionary<Transform, ConstructionBuilder>();
		_numUserConstructions = PlayerPrefs.GetInt("numUserConstructions", 0);
		resourcesProvider = Resources.Load<ConstructionResourcesProvider> ("ConstructionResourcesProvider");
		resourcesProvider.Init();
	}

	public ConstructionBuilder SetConstruction(Transform pContainer){
		return string.IsNullOrEmpty(_currentConstructionData) ? null : SetConstruction(_currentConstructionData, pContainer);
	}

	public ConstructionBuilder SetConstruction(IList<BrickData> allBricks, Transform pContainer){
		var builder = pContainer.gameObject.AddComponent<ConstructionBuilder>();
		builder.construction = new LXFMLConstruction(allBricks);
		_builders.Add(pContainer, builder);		
		return builder;
	}

	public ConstructionBuilder SetConstruction(string xml, Transform pContainer){
		var builder = pContainer.gameObject.AddComponent<ConstructionBuilder>();
		builder.construction = LXFMLParser.ParseConstruction(xml, ConstructionType.Building);
		_currentConstructionData = xml;
		_builders.Add(pContainer, builder);
		return builder;
	}

	public void DestroyTemporaryConstruction(Transform pContainer){
		if(_builders.ContainsKey(pContainer))
		{
			ConstructionBuilder builder = _builders[pContainer];
			builder.Cleanup();
			_builders.Remove(pContainer);
		}
	}

	public void ClearUserConstruction(int index){
		PlayerPrefs.DeleteKey("construction_" + index.ToString());
		File.Delete(Application.persistentDataPath + Path.DirectorySeparatorChar + "construction_screenshot_" + index.ToString() + ".png");
		for(int i = index + 1 ; i < _numUserConstructions ; i++){
			string oldModel = PlayerPrefs.GetString("construction_" + i.ToString());
			PlayerPrefs.SetString("construction_" + (i-1).ToString(), oldModel);
			PlayerPrefs.DeleteKey("construction_" + i.ToString());
			string from = Application.persistentDataPath + Path.DirectorySeparatorChar + "construction_screenshot_" + i.ToString() + ".png";
			string to = Application.persistentDataPath + Path.DirectorySeparatorChar + "construction_screenshot_" + (i-1).ToString() + ".png";
			File.Move(from, to);

		}
		_numUserConstructions--;
		PlayerPrefs.SetInt("numUserConstructions", _numUserConstructions);
	}

	public void ClearAllUserConstructions(){
		_numUserConstructions = PlayerPrefs.GetInt("numUserConstructions", 0);
		for(int i = 0 ; i < _numUserConstructions ; i++){
			PlayerPrefs.DeleteKey("construction_" + i.ToString());
			File.Delete(Application.persistentDataPath + Path.DirectorySeparatorChar + "construction_screenshot_" + i.ToString() + ".png");
		}
		PlayerPrefs.SetInt("numUserConstructions", 0);
		_numUserConstructions = 0;
	}

	public ConstructionBuilder GetBuilderByTarget(Transform target){
		return _builders[target];
	}

	public ConstructionBuilder SetUserConstruction(Transform target){
		return SetUserConstruction(_numUserConstructions - 1, target);
	}

	public ConstructionBuilder SetUserConstruction(int index, Transform target){
		if(_numUserConstructions > 0){
			if(_numUserConstructions >= index){
				string serialized = PlayerPrefs.GetString("construction_" + index.ToString());
				if(!string.IsNullOrEmpty(serialized)){
					IList<BrickData> bricks = ConstructionSerializer.DeserializeConstruction(serialized);
					return SetConstruction(bricks, target);
				}
			}else{
				UnityEngine.Debug.LogFormat ("no user construction for index {0}", index);
			}
		}else{
			Debug.Log ("no user construction");
		}
		return null;
	}

	public Sprite[] GetConstructionThumbnails(){
		_numUserConstructions = PlayerPrefs.GetInt("numUserConstructions");
		Sprite[] sprites = new Sprite[_numUserConstructions];
		for(int i = 0 ; i < _numUserConstructions ; i++){
			try{
				byte[] imgBytes = File.ReadAllBytes(Application.persistentDataPath + Path.DirectorySeparatorChar + "construction_screenshot_" + i.ToString() + ".png");
				Texture2D img = new Texture2D(128,128);
				img.LoadImage(imgBytes);
				sprites[i] = Sprite.Create(img,new Rect(0f,0f,128f,128f), new Vector2(0.5f,0.5f));
			}catch{
				sprites[i] = new Sprite();
			}
		}
		return sprites;
	}

	public void SaveConstruction(Transform constructionTarget){
		var builder = _builders[constructionTarget];
		if(SaveConstructionData(builder))
			SaveConstructionScreenshot(builder, constructionTarget);
	}

	void SaveConstructionScreenshot(ConstructionBuilder builder, Transform constructTarget){
		builder.SetConstructionPivot(ConstructionBuilder.ConstructionPivot.MiddleCenter);
		Vector3 pos = constructTarget.position;
		Quaternion rot = constructTarget.rotation;
		constructTarget.Translate(100f,0f,0f);
		ScreenshotCamera screenshot = FindObjectOfType<ScreenshotCamera>();
		Assert.IsNotNull<ScreenshotCamera>(screenshot,"Screenshot Camera should not be null");
		screenshot.TakeScreenshot(_numUserConstructions - 1);
		constructTarget.position = pos;
		constructTarget.rotation = rot;
	}

	bool SaveConstructionData(ConstructionBuilder builder){
		string serialized = ConstructionSerializer.SerializeConstruction(builder.construction.GetAllBricks());
		_numUserConstructions = PlayerPrefs.GetInt("numUserConstructions", 0);
		string constructionId = "construction_" + _numUserConstructions.ToString();
		bool success = false;
		try{
			PlayerPrefs.SetString(constructionId, serialized);
			success = true;
		}catch {
			Debug.LogError ("Unable to save construction");
		}
		if(success){
			_numUserConstructions++;
			PlayerPrefs.SetInt("numUserConstructions", _numUserConstructions);
		}
		return success;
	}
}
