using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class ConstructionResourcesProvider : ScriptableObject
{
	static readonly Regex kPrefabPattern = new Regex(@"_(\d+)$");
	static readonly Regex kMaterialPattern = new Regex(@"^(\d+)-");

	[SerializeField]
	GameObject[] _brickPrefabs;
	public GameObject[] AllBricks{
		get{
			return _brickPrefabs;
		}
	}

	[SerializeField]
	Material[] _brickMaterials;

	[SerializeField]
	GameObject[] _decorations;

	Dictionary<int, GameObject> _prefabsLookup = new Dictionary<int, GameObject>();
	Dictionary<int, Material> _materialsLookup = new Dictionary<int, Material>();

	public void Init()
	{

		if(_brickMaterials.Length == 0 || _brickPrefabs.Length == 0){
			Debug.LogError("No bricks ");
		}

		Match match;

		foreach (var prefab in _brickPrefabs)
		{
			match = kPrefabPattern.Match(prefab.name);

			if (match.Success)
			{
				int designId;

				if (int.TryParse(match.Groups[1].ToString(), out designId) == false)
				{
					continue;
				}

				_prefabsLookup.Add(designId, prefab);
			}
		}

		foreach (var material in _brickMaterials)
		{
			match = kMaterialPattern.Match(material.name);
			
			if (match.Success)
			{
				int designId;
				
				if (int.TryParse(match.Groups[1].ToString(), out designId) == false)
				{
					continue;
				}
				
				_materialsLookup.Add(designId, material);
			}
		}
	}

	public GameObject GetDecorationForWidth(int width, LXFMLDecoration.DecoPosition spot){
		List<GameObject> decos = new List<GameObject>();

		for(int i = 0 ; i < _decorations.Length ; i++){
			LXFMLDecoration deco = _decorations[i].GetComponent<LXFMLDecoration>();
			if(deco.position == spot && deco.width <= width){
				decos.Add(_decorations[i]);
			}
		}
		if(decos.Count == 0)
			return null;
		return decos[Random.Range(0,decos.Count)];
	}
	public GameObject GetDecorationForHeight(List<int> heights, LXFMLDecoration.DecoPosition spot){
		List<GameObject> decos = new List<GameObject>();
		for(int i = 0 ; i < _decorations.Length ; i++){
			LXFMLDecoration deco = _decorations[i].GetComponent<LXFMLDecoration>();
			if(deco.position == spot){
				for(int j = 0 ; j < heights.Count ; j++){
					if(deco.height == (float)heights[j] ){
						decos.Add(_decorations[i]);
					}
				}
			}
		}
		if(decos.Count == 0)
			return null;
		return decos[Random.Range(0,decos.Count)];
	}

//	public GameObject GetDecorationForHeight(float height){
//		List<GameObject> decos = new List<GameObject>();
//		for(int i = 0 ; i < _decorations.Length ; i++){
//			LXFMLDecoration deco = _decorations[i].GetComponent<LXFMLDecoration>();
//			if(deco.height <= height ){
//				decos.Add(_decorations[i]);
//			}
//		}
//		if(decos.Count == 0)
//			return null;
//		return decos[Random.Range(0,decos.Count)];
//	}

	public List<int> GetAvailableBricksForFootprint(Vector4 footprint, int pDesignID){
		List<int> availBricks = new List<int>();
		
		int width = (int)(footprint.x + footprint.y) + 1;
		int height = (int)(footprint.z + footprint.w) + 1;

		foreach(KeyValuePair<int, GameObject> brick in _prefabsLookup){
			int designId = brick.Key;
			if(LXFMLHelper.GetBrickHeight(designId) <= height && LXFMLHelper.GetBrickWidth(designId) <= width && designId != pDesignID && !(LXFMLHelper.IsSpecial2By1Brick(designId)) && !(LXFMLHelper.Is2By1Brick(designId) && LXFMLHelper.Is2By1Brick(pDesignID)))
				availBricks.Add(brick.Key);
		}
		
		return availBricks;
	}

	public GameObject GetPrefabForDesign(int designId)
	{
		if (_prefabsLookup.ContainsKey(designId))
		{
			return _prefabsLookup[designId];
		}

		return null;
	}

	public Material GetMaterial(int materialId)
	{
		if (_materialsLookup.ContainsKey(materialId))
		{
			return _materialsLookup[materialId];
		}
		
		return null;
	}
}
