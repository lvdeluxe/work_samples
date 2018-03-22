using UnityEngine;
using System.Collections;

public class FakeExtrusionAnimation : MonoBehaviour {

	public GameObject terrainPrefab;
	public GameObject archPrefab;
	public GameObject scanStationPrefab;

	private GameObject _terrain;
	private GameObject _arch;
	private GameObject _archScreen;
	private GameObject _scanStation;
	private GameObject _terrainMesh;

	private ConstructionBuilder _builder;

	private Vector3 _referencePoint;

	private MeshRenderer _vfxPlaneRenderer;


	public void StartAnimation(ConstructionBuilder builder){
		_builder = builder;
		_referencePoint = _builder.GetActualConstructionPivot();
		StartCoroutine (ExtrusionAnimation());
	}

	IEnumerator ExtrusionAnimation () {
		yield return StartCoroutine(TransitionFromAR());
		yield return StartCoroutine(ArchAnimation());
		yield return StartCoroutine(TerrainAnimation());
		StartCoroutine(SpinConstruction());
		Debug.Log ("ExtrusionAnimation complete");
	}

	IEnumerator TransitionFromAR(){

		_scanStation = Instantiate(scanStationPrefab) as GameObject;
		_scanStation.transform.SetParent(_builder.gameObject.transform);
		_scanStation.transform.localPosition = _referencePoint;
		_scanStation.transform.SetParent(null);
		_referencePoint = _scanStation.transform.position;

		yield return new WaitForSeconds(2f);



		iTween.MoveBy(_builder.gameObject,new Vector3(0f,5f,0f), 1f);

		yield return new WaitForSeconds(0.3f);

		iTween.MoveBy(_scanStation,new Vector3(0f,0f,200f), 0.7f);

		yield return new WaitForSeconds(0.5f);
	}
	
	IEnumerator ArchAnimation(){
		_arch = Instantiate(archPrefab) as GameObject;
		_arch.transform.localPosition = _referencePoint;

		_archScreen = _arch.transform.FindChild("Building_station_arch").gameObject;

		_vfxPlaneRenderer = _archScreen.transform.FindChild("Building_station_vfx_plane").GetComponent<MeshRenderer>();
		_vfxPlaneRenderer.enabled = false;

		_arch.transform.Translate(0f,0f,-50f,Space.Self);

		iTween.MoveBy(_arch,new Vector3(0f,0f,50f), 1f);
		
		yield return new WaitForSeconds(1f);

		iTween.MoveBy(_builder.gameObject,new Vector3(0f,-5f,0f), 0.5f);

		yield return new WaitForSeconds(1f);

		iTween.MoveBy(_archScreen,iTween.Hash("time", 3f,"amount", new Vector3(0f,0f,13.85f), "easetype", iTween.EaseType.linear, "onupdate", "OnUpdateScanArch", "onupdatetarget", this.gameObject));

		yield return new WaitForSeconds(3f);

		iTween.MoveBy(_builder.gameObject,new Vector3(0f,5f,0f), 1f);
		
		yield return new WaitForSeconds(0.3f);
		
		iTween.MoveBy(_arch,new Vector3(0f,0f,200f), 0.7f);
		
		yield return new WaitForSeconds(0.5f);

	}

	public void OnUpdateScanArch(){
		float zStart = -0.4f;
		float zEnd = zStart + (12f * 0.8f);
		if(_archScreen.transform.localPosition.z > zStart && _archScreen.transform.localPosition.z < zEnd){
			float currentZ = _archScreen.transform.localPosition.z - zStart;

			int sliceIndex = Mathf.FloorToInt(currentZ / zEnd * 11f);
			_builder.EnableBricksByExtrusionIndex(sliceIndex);
			_vfxPlaneRenderer.enabled = true;

		}else{
			_vfxPlaneRenderer.enabled = false;
		}
	}

	IEnumerator TerrainAnimation(){
		yield return 0;
		_terrain = Instantiate(terrainPrefab) as GameObject;
		_terrain.transform.localPosition = _referencePoint;

		_terrain.transform.Translate(0f,0f,-50f,Space.Self);

		iTween.MoveBy(_terrain,new Vector3(0f,0f,50f), 1f);
		
		yield return new WaitForSeconds(0.7f);
		
		iTween.MoveBy(_builder.gameObject,new Vector3(0f,-5f,0f), 0.5f);
		
		yield return new WaitForSeconds(0.5f);

		_builder.ShowDecorations();
	}

	IEnumerator SpinConstruction(){
		GameObject container = new GameObject("container");

		_terrainMesh = _terrain.transform.FindChild("Terrain_platform/Material_153").gameObject;

		Debug.Log (_terrainMesh.transform.position);

		container.transform.position = _terrainMesh.transform.position;

		_terrain.transform.SetParent(container.transform);
		_builder.transform.SetParent(container.transform);

		iTween.RotateBy(container,iTween.Hash("amount", new Vector3(0f,1f, 0f), "time", 8f, "easetype", iTween.EaseType.easeOutQuad));

		yield return new WaitForSeconds(8f);
	}
}
