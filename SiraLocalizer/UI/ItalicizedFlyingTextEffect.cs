using UnityEngine;

namespace SiraLocalizer.UI
{
	internal class ItalicizedFlyingTextSpawner : FlyingTextSpawner
	{
		public void Awake()
		{
			_duration = 0.7f;
			_xSpread = 2f;
			_targetYPos = 1.3f;
			_targetZPos = 14f;
			_color = Color.white;
		}
	}
}
