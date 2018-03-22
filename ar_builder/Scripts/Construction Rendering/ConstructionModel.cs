using UnityEngine;
using System.Collections;

public class ConstructionModel : MonoBehaviour 
{
	public TextAsset _constructionData;

	[SerializeField]
	bool _dumpData;

	public LXFMLConstruction Construction
	{
		get { return _construction; }
	}

	LXFMLConstruction _construction;

	public void Init()
	{
		ConstructionBuilder builder = ConstructionController.Instance.SetConstruction(_constructionData.text, transform);
		builder.Init(ConstructionBuilder.BuilderType.Scan);
		builder.CreateBuilding();

		FindObjectOfType<FakeExtrusionAnimation>().StartAnimation(builder);


		if (_dumpData)
		{
			_construction.Grid.Dump();
		}
	}
}
