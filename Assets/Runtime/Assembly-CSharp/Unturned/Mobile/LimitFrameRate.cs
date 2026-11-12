using UnityEngine;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Target frame rate limiter for mobile devices to manage thermals and battery.
	/// </summary>
	public class LimitFrameRate : MonoBehaviour
	{
		public int limitFrameRate = 60;

		private void Start()
		{
			Application.targetFrameRate = limitFrameRate;
		}
	}
}
