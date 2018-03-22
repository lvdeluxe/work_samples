using System.Xml;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public static class LXFMLParser
{
	const string kTokenBrickId = "refID";
	const string kTokenMaterials = "materials";
	const string kTokenDesign = "designID";
	const string kTokenBrickMatrix = "transformation";

	public static LXFMLConstruction ParseConstruction(string lxfmlData, ConstructionType pType)
	{
		var construction = GetConstructionByType(pType);

		XmlDocument xmlDoc = new XmlDocument();

		xmlDoc.LoadXml(lxfmlData);

		XmlNodeList nodes = xmlDoc.SelectNodes("LXFML/Bricks/Brick/Part");
		BrickData brick;

		float minX = float.MaxValue;
		float minY = float.MaxValue;

		List<BrickData> list_tmp = new List<BrickData>();

		foreach(XmlElement node in nodes)
		{
			brick = new BrickData();
			brick.design = new BrickDesignData();

			brick.id =  int.Parse(node.GetAttribute(kTokenBrickId));
			brick.materialId =  int.Parse(node.GetAttribute(kTokenMaterials).Split(',')[0]);
			brick.design.id = int.Parse(node.GetAttribute(kTokenDesign));
			brick.design.width = LXFMLHelper.GetBrickWidth(brick.design.id);
			brick.design.height = LXFMLHelper.GetBrickHeight(brick.design.id);
			brick.design.type = LXFMLHelper.GetBrickType(brick.design.id);

			Matrix4x4 matrix = LXFMLHelper.GetBrickMatrix((node.FirstChild as XmlElement).GetAttribute(kTokenBrickMatrix));
			brick.position = LXFMLHelper.GetBrickPosition(matrix);

			minX = Mathf.Min(minX, brick.position.x);
			minY = Mathf.Min(minY, brick.position.y);

			brick.scale = LXFMLHelper.GetBrickScale(matrix);
			brick.rotation = LXFMLHelper.GetBrickRotation(matrix);

			brick.isFlipped = LXFMLHelper.IsBrickFlipped(brick);

			list_tmp.Add(brick);
		}

		if (minX > 0) {
			minX = 0;
		}

		if (minY > 0) {
			minY = 0;
		}

		//Offset position
		foreach(BrickData b in list_tmp){
//			try
//			{
				var pos = b.position;
				pos.x -= minX;
				pos.y -= minY;
				b.position = pos;
				construction.AddBrick(b);
//			}
//			catch(Exception e)
//			{
//				UnityEngine.Debug.LogError(b.position + " " + minX + " " + minY);
//			}
		}

		return construction;
	}
	

	private static LXFMLConstruction GetConstructionByType(ConstructionType pType){
		switch(pType){
			case ConstructionType.Building:
				return new LXFMLBuildingConstruction(BuildingTypes.AbstractType1);
			default:
			case ConstructionType.Vehicle:
				return new LXFMLVehicleConstruction();
		}
	}
}
