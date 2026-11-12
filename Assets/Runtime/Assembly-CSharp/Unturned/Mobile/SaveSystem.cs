using System.IO;
using UnityEngine;

namespace SDG.Unturned.Mobile
{
	public static class SaveSystem
	{
		private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "ButtonsData.json");

		public static void SaveGame(ButtonsData buttonsData)
		{
			if (buttonsData == null) return;
			try
			{
				string json = JsonUtility.ToJson(buttonsData, true);
				File.WriteAllText(SaveFilePath, json);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[SDG.Unturned.Mobile.SaveSystem] Failed to save ButtonsData: {ex}");
			}
		}

		public static ButtonsData LoadData()
		{
			if (!File.Exists(SaveFilePath)) return null;
			try
			{
				string json = File.ReadAllText(SaveFilePath);
				return JsonUtility.FromJson<ButtonsData>(json);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"[SDG.Unturned.Mobile.SaveSystem] Failed to load ButtonsData: {ex}");
				return null;
			}
		}
	}
}
