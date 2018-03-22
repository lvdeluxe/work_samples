using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

[Serializable]
public class BrickDataSerializer: ISerializable{
	
	public int id;
	public int materialId;
	public bool isFlipped;
	
	public float positionX;
	public float positionY;
	public float positionZ;
	
	public float scaleX;
	public float scaleY;
	public float scaleZ;
	
	public float rotationX;
	public float rotationY;
	public float rotationZ;
	public float rotationW;
	
	public int designId;
	public int designWidth ;
	public int designHeight ;
	
	public BrickType designType;
	
	public BrickDataSerializer(){
		
	}
	
	public void SetFromBrickData(BrickData brickData){
		id = brickData.id;
		materialId = brickData.materialId;
		isFlipped = brickData.isFlipped;
		positionX = brickData.position.x;
		positionY = brickData.position.y;
		positionZ = brickData.position.z;
		scaleX = brickData.scale.x;
		scaleY = brickData.scale.y;
		scaleZ = brickData.scale.z;
		rotationX = brickData.rotation.x;
		rotationY = brickData.rotation.y;
		rotationZ = brickData.rotation.z;
		rotationW = brickData.rotation.w;
		designId = brickData.design.id;
		designWidth = brickData.design.width;
		designHeight = brickData.design.height;
		designType = brickData.design.type;
	}
	
	public BrickData GetBrickData(){
		var brick = new BrickData();
		brick.design = new BrickDesignData();		
		brick.id =  id;
		brick.materialId =  materialId;
		brick.design.id = designId;
		brick.design.width = designWidth;
		brick.design.height = designHeight;
		brick.design.type = designType;
		brick.position = new Vector3(positionX,positionY,positionZ);
		brick.scale = new Vector3(scaleX,scaleY,scaleZ);
		brick.rotation = new Quaternion(rotationX,rotationY,rotationZ,rotationW);		
		brick.isFlipped = isFlipped;
		return brick;
	}
	
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("id", id, typeof(int));
		info.AddValue("materialId", materialId, typeof(int));
		info.AddValue("isFlipped", isFlipped, typeof(bool));
		info.AddValue("positionX", positionX, typeof(float));
		info.AddValue("positionY", positionY, typeof(float));
		info.AddValue("positionZ", positionZ, typeof(float));
		info.AddValue("scaleX", scaleX, typeof(float));
		info.AddValue("scaleY", scaleY, typeof(float));
		info.AddValue("scaleZ", scaleZ, typeof(float));
		info.AddValue("rotationX", rotationX, typeof(float));
		info.AddValue("rotationY", rotationY, typeof(float));
		info.AddValue("rotationZ", rotationZ, typeof(float));
		info.AddValue("rotationW", rotationW, typeof(float));
		info.AddValue("designId", designId, typeof(int));
		info.AddValue("designWidth", designWidth, typeof(int));
		info.AddValue("designHeight", designHeight, typeof(int));
		info.AddValue("designType", designType, typeof(BrickType));		
	}
	
	public BrickDataSerializer(SerializationInfo info, StreamingContext context)
	{
		id = (int) info.GetValue("id", typeof(int));
		materialId = (int) info.GetValue("materialId", typeof(int));
		isFlipped = (bool) info.GetValue("isFlipped", typeof(bool));
		positionX = (float) info.GetValue("positionX", typeof(float));
		positionY = (float) info.GetValue("positionY", typeof(float));
		positionZ = (float) info.GetValue("positionZ", typeof(float));
		scaleX = (float) info.GetValue("scaleX", typeof(float));
		scaleY = (float) info.GetValue("scaleY", typeof(float));
		scaleZ = (float) info.GetValue("scaleZ", typeof(float));
		rotationX = (float) info.GetValue("rotationX", typeof(float));
		rotationY = (float) info.GetValue("rotationY", typeof(float));
		rotationZ = (float) info.GetValue("rotationZ", typeof(float));
		rotationW = (float) info.GetValue("rotationW", typeof(float));
		designId = (int) info.GetValue("designId", typeof(int));
		designWidth = (int) info.GetValue("designWidth", typeof(int));
		designHeight = (int) info.GetValue("designHeight", typeof(int));
		designType = (BrickType) info.GetValue("designType", typeof(BrickType));
	}
	
}

public static class ConstructionSerializer {

	public static string SerializeConstruction(IList<BrickData> construction)
	{
		BrickDataSerializer[] constructionSerialized = new BrickDataSerializer[construction.Count];
		for(int i = 0 ; i < construction.Count ; i++){
			BrickDataSerializer t = new BrickDataSerializer();
			t.SetFromBrickData(construction[i]);
			constructionSerialized[i] = t;
		}

		Stream stream = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (stream, constructionSerialized);
		
		stream.Seek (0, SeekOrigin.Begin);
		byte[] stateData = new byte[stream.Length];
		stream.Read (stateData, 0, (int)stream.Length);
		stream.Close ();
		
		return System.Convert.ToBase64String (stateData);
	}
	
	
	public static IList<BrickData> DeserializeConstruction(string serialized)
	{
		byte[] stateData = System.Convert.FromBase64String (serialized);
		Stream stream = new MemoryStream (stateData);
		BinaryFormatter bf = new BinaryFormatter ();
		BrickDataSerializer[] constructionDeserialized = null;
		
		try {
			constructionDeserialized = (BrickDataSerializer[])bf.Deserialize (stream);
		} catch {
			Debug.LogError ("Unable to deserialize data");
		}
		
		stream.Close (); 
		List<BrickData> construction = new List<BrickData>();
		if(constructionDeserialized != null){
			for(int i = 0 ; i < constructionDeserialized.Length ; i++){
				construction.Add(constructionDeserialized[i].GetBrickData());
			}
		}
		return construction;
	}  

}
