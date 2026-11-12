using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SDG.Unturned;

namespace SDG.Unturned.Mobile
{
	/// <summary>
	/// Generates and manages the responsive mobile touch HUD layout.
	/// Coordinates button layout, virtual joystick, touchpad look controls, and responsive UI scaling.
	/// </summary>
	public class MobileHUDBuilder : MonoBehaviour
	{
		private static MobileHUDBuilder _instance;
		public static MobileHUDBuilder Instance => _instance;

		public static bool IsAnyUIOpen()
		{
			if (string.Equals(SceneManager.GetActiveScene().name, "Menu")) return true;
			if (Player.player == null) return true;
			if (PlayerUI.window != null && PlayerUI.window.showCursor) return true;
			if (PlayerDashboardUI.active) return true;
			if (PlayerPauseUI.active) return true;
			if (PlayerDeathUI.active) return true;
			if (PlayerLifeUI.chatting) return true;
			if (PlayerLifeUI.gesturing) return true;
			if (PlayerNPCDialogueUI.active) return true;
			if (PlayerNPCVendorUI.active) return true;
			if (PlayerNPCQuestUI.active) return true;
			if (PlayerBarricadeSignUI.active) return true;
			if (PlayerBarricadeLibraryUI.active) return true;
			return false;
		}

		public static bool IsInventoryOpen()
		{
			return PlayerDashboardUI.active;
		}

		public static bool IsWeaponInspectionOpen()
		{
			if (Player.player != null && Player.player.equipment != null && Player.player.equipment.useable is UseableGun gun)
			{
				return gun.isInspectingAttachments;
			}
			return false;
		}

		public static bool ShouldShowTabButton()
		{
			return IsInventoryOpen() || IsWeaponInspectionOpen();
		}

		public static bool IsInVehicle()
		{
			return Player.player != null && Player.player.movement != null && Player.player.movement.getVehicle() != null;
		}

		public static bool IsGlobalButton(string name)
		{
			return name == "Inventory" || name == "Menu" || name == "Chat" || name == "H" || name == "Voice" || name == "HUD" || name == "LookTouchPad_8tdg5543ffre";
		}

		private void Awake()
		{
			EnsureHUDExists();
		}

		public static void EnsureHUDExists()
		{
#if UNITY_ANDROID
			Input.multiTouchEnabled = true;
			Input.simulateMouseWithTouches = true;
#endif

			// Ensure active EventSystem exists in the scene so touch inputs reach UGUI handlers
			if (EventSystem.current == null)
			{
				GameObject esGo = new GameObject("EventSystem");
				DontDestroyOnLoad(esGo);
				esGo.AddComponent<EventSystem>();
				StandaloneInputModule sim = esGo.AddComponent<StandaloneInputModule>();
				sim.forceModuleActive = true;
			}
			else
			{
				StandaloneInputModule sim = EventSystem.current.GetComponent<StandaloneInputModule>();
				if (sim != null)
				{
					sim.forceModuleActive = true;
				}
			}

			if (_instance != null && _instance.gameObject != null) return;
			if (GameObject.Find("MobileHUDCanvas") != null) return;

			GameObject rootGo = new GameObject("MobileHUDCanvas");
			DontDestroyOnLoad(rootGo);
			_instance = rootGo.AddComponent<MobileHUDBuilder>();
			_instance.BuildMobileHUD();
		}

		private void LateUpdate()
		{
			InputBroker.ClearFrameDeltas();
		}

		private void BuildMobileHUD()
		{
			// 1. Root Canvas
			Canvas canvas = gameObject.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 30000;

			CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
			scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			scaler.referenceResolution = new Vector2(1920f, 1080f);
			scaler.matchWidthOrHeight = 0.5f;

			gameObject.AddComponent<GraphicRaycaster>();
			ControllerManager controllerMgr = gameObject.AddComponent<ControllerManager>();
			SaveManager saveMgr = gameObject.AddComponent<SaveManager>();
			LimitFrameRate frameLimiter = gameObject.AddComponent<LimitFrameRate>();

			Color hudColor = new Color(1f, 1f, 1f, 0.39215686f);

			// 2. Child 00: Joystick Touch Pad (covers left 40% of screen, middle vertical band)
			GameObject joyZoneGo = CreateUIElement("Joystick Touch Pad", transform,
				new Vector2(0f, 0.20f), new Vector2(0.4f, 0.75f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
			Image joyZoneImg = joyZoneGo.AddComponent<Image>();
			joyZoneImg.color = Color.clear;
			joyZoneImg.raycastTarget = true;

			// Joystick Background
			GameObject joyBgGo = CreateUIElement("Joystick Background", joyZoneGo.transform,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -150f), new Vector2(250f, 250f), new Vector2(0.5f, 0.5f));
			Image joyBgImg = joyBgGo.AddComponent<Image>();
			joyBgImg.sprite = MobileSpriteProvider.GetSprite("lineLight09");
			joyBgImg.color = hudColor;
			joyBgImg.raycastTarget = false;

			// Joystick Handle
			GameObject joyHandleGo = CreateUIElement("Handle", joyBgGo.transform,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f), new Vector2(0.5f, 0.5f));
			Image joyHandleImg = joyHandleGo.AddComponent<Image>();
			joyHandleImg.sprite = MobileSpriteProvider.GetSprite("lineLight09");
			joyHandleImg.color = hudColor;
			joyHandleImg.raycastTarget = false;

			FloatingJoyStick floatingJoy = joyZoneGo.AddComponent<FloatingJoyStick>();
			floatingJoy.joystickBackground = joyBgGo.GetComponent<RectTransform>();
			floatingJoy.joyStick = joyHandleGo.GetComponent<RectTransform>();
			floatingJoy.joystickMovementRange = 1f;
			floatingJoy.normalColor = hudColor;
			floatingJoy.pressedColor = new Color(1f, 1f, 1f, 1f);

			CanvasGroup joyCg = joyZoneGo.AddComponent<CanvasGroup>();
			Patterns joyPat = joyZoneGo.AddComponent<Patterns>();
			joyPat.m_ButtonPattern = ButtonPattern.None;
			joyPat.m_CanvasGroup = joyCg;

			// 3. Child 01: LookTouchPad_8tdg5543ffre (covers right 60% of screen)
			GameObject lookZoneGo = CreateUIElement("LookTouchPad_8tdg5543ffre", transform,
				new Vector2(0.4f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
			Image lookImg = lookZoneGo.AddComponent<Image>();
			lookImg.color = Color.clear;
			lookImg.raycastTarget = true;
			UltimateTouchpad touchpad = lookZoneGo.AddComponent<UltimateTouchpad>();
			touchpad.touchpadName = "LookTouchPad_8tdg5543ffre";
			touchpad.xSensitivity = 0.1f;
			touchpad.ySensitivity = 0.05f;
			touchpad.screenWidthReference = 1280f;
			touchpad.xGravity = 0.5f;
			touchpad.yGravity = 0.5f;
			touchpad.useTouchInput = false;
			lookZoneGo.AddComponent<UltimateTouchpadScreenSizeUpdater>();

			CanvasGroup lookCg = lookZoneGo.AddComponent<CanvasGroup>();
			Patterns lookPat = lookZoneGo.AddComponent<Patterns>();
			lookPat.m_ButtonPattern = ButtonPattern.None;
			lookPat.m_CanvasGroup = lookCg;

			// 4. Build all 41 mobile buttons
			BuildMobileButtons(transform, hudColor);

			// 5. SliderSizeRegulator_048gr3gerbr5535g
			CreateSizeSlider(transform, controllerMgr);
		}

		private struct MobileButtonSpec
		{
			public string name;
			public float aminX, aminY, amaxX, amaxY;
			public float posX, posY;
			public float sizeW, sizeH;
			public KeyCode keyCode;
			public bool lookSimulate;
			public ButtonPattern pattern;
			public string bgSprite;
			public string iconSprite;
			public float iconRotZ;
			public float iconSizeOffsetW, iconSizeOffsetH;
			public string label;

			public MobileButtonSpec(string n, float amnx, float amny, float amxx, float amxy,
				float px, float py, float sw, float sh, KeyCode key, bool look, ButtonPattern pat,
				string bg, string icon, float rot, float isw, float ish, string lbl)
			{
				name = n;
				aminX = amnx; aminY = amny; amaxX = amxx; amaxY = amxy;
				posX = px; posY = py;
				sizeW = sw; sizeH = sh;
				keyCode = key;
				lookSimulate = look;
				pattern = pat;
				bgSprite = bg;
				iconSprite = icon;
				iconRotZ = rot;
				iconSizeOffsetW = isw;
				iconSizeOffsetH = ish;
				label = lbl;
			}
		}

		private void BuildMobileButtons(Transform parent, Color hudColor)
		{
			MobileButtonSpec[] specs = new MobileButtonSpec[]
			{
				new MobileButtonSpec("Fire", 1.0f, 0.0f, 1.0f, 0.0f, -286.0f, 176.0f, 200.0f, 200.0f, KeyCode.Mouse0, true, ButtonPattern.None, "lineLight09", "pvp", 135f, -60.0f, -60.0f, ""),
				new MobileButtonSpec("Aim", 1.0f, 0.0f, 1.0f, 0.0f, -107.7998046875f, 304.5f, 150.0f, 150.0f, KeyCode.Mouse1, true, ButtonPattern.None, "lineLight09", "skills", 0f, -60.0f, -60.0f, ""),
				new MobileButtonSpec("Inventory", 0.0f, 1.0f, 0.0f, 1.0f, 61.0f, -64.0f, 100.0f, 100.0f, KeyCode.Tab, false, ButtonPattern.None, "lineLight06", "", 0f, 0.0f, 0.0f, "Tab"),
				new MobileButtonSpec("Crouch", 1.0f, 0.0f, 1.0f, 0.0f, -57.2001953125f, 60.0f, 75.0f, 75.0f, KeyCode.X, false, ButtonPattern.None, "lineLight09", "Fold_Out", -90f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("Run", 1.0f, 0.0f, 1.0f, 0.0f, -243.89999389648438f, 498.29998779296875f, 165.77000427246094f, 166.0f, KeyCode.LeftShift, true, ButtonPattern.None, "lineLight09", "go", 90f, -60.0f, -60.0f, ""),
				new MobileButtonSpec("Menu", 1.0f, 1.0f, 1.0f, 1.0f, -87.5f, -73.0f, 100.0f, 100.0f, KeyCode.Escape, false, ButtonPattern.None, "lineLight06", "configure", 0f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("Jump", 1.0f, 0.0f, 1.0f, 0.0f, -61.69970703125f, 454.70001220703125f, 100.0f, 100.0f, KeyCode.Space, false, ButtonPattern.None, "lineLight09", "Fold_Out", 90f, -50.0f, -50.0f, ""),
				new MobileButtonSpec("Prone", 1.0f, 0.0f, 1.0f, 0.0f, -156.0f, 60.0f, 75.0f, 75.0f, KeyCode.Z, false, ButtonPattern.None, "lineLight09", "Fold_Out", -90f, 0.0f, -42.1299934387207f, ""),
				new MobileButtonSpec("Reload", 1.0f, 0.0f, 1.0f, 0.0f, -92.115234375f, 587.0f, 125.0f, 125.0f, KeyCode.R, false, ButtonPattern.None, "lineLight09", "refresh", 45f, -50.0f, -50.0f, ""),
				new MobileButtonSpec("Firemode", 1.0f, 1.0f, 1.0f, 1.0f, -331.10003662109375f, -73.0f, 100.0f, 100.0f, KeyCode.V, false, ButtonPattern.None, "lineLight09", "fullPlayers", 180f, -60.0f, -60.0f, ""),
				new MobileButtonSpec("Attachment", 1.0f, 1.0f, 1.0f, 1.0f, -450.0f, -73.0f, 100.0f, 100.0f, KeyCode.T, false, ButtonPattern.None, "lineLight09", "item", 0f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("Interact", 1.0f, 0.0f, 1.0f, 0.0f, -338.5f, 352.79998779296875f, 125.0f, 125.0f, KeyCode.F, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "F"),
				new MobileButtonSpec("C", 0.0f, 1.0f, 0.0f, 1.0f, 165.0f, -62.5f, 75.0f, 75.0f, KeyCode.C, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "C"),
				new MobileButtonSpec("Voice", 1.0f, 1.0f, 1.0f, 1.0f, -330.0f, -186.0f, 75.0f, 75.0f, KeyCode.LeftAlt, false, ButtonPattern.None, "lineLight09", "voice", 0f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("Q", 1.0f, 1.0f, 1.0f, 1.0f, -438.7999267578125f, -186.00003051757812f, 100.0f, 100.0f, KeyCode.Q, true, ButtonPattern.None, "lineLight09", "Fold_Out", 180f, -50.0f, -50.0f, ""),
				new MobileButtonSpec("E", 1.0f, 1.0f, 1.0f, 1.0f, -220.0f, -186.0f, 100.0f, 100.0f, KeyCode.E, true, ButtonPattern.None, "lineLight09", "Fold_Out", 0f, -50.0f, -50.0f, ""),
				new MobileButtonSpec("1", 1.0f, 0.0f, 1.0f, 0.0f, -812.0000610351562f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha1, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "1"),
				new MobileButtonSpec("2", 1.0f, 0.0f, 1.0f, 0.0f, -712.0f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha2, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "2"),
				new MobileButtonSpec("3", 1.0f, 0.0f, 1.0f, 0.0f, -612.0000610351562f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha3, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "3"),
				new MobileButtonSpec("4", 1.0f, 0.0f, 1.0f, 0.0f, -512.0000610351562f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha4, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "4"),
				new MobileButtonSpec("5", 1.0f, 0.0f, 1.0f, 0.0f, -412.00006103515625f, 53.0f, 75.0f, 75.0f, KeyCode.Alpha5, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "5"),
				new MobileButtonSpec("N", 0.0f, 1.0f, 0.0f, 1.0f, 347.00006103515625f, -62.5f, 75.0f, 75.0f, KeyCode.N, false, ButtonPattern.None, "lineLight09", "", 0f, -30.000001907348633f, -30.000001907348633f, "N"),
				new MobileButtonSpec("f1", 0.0f, 1.0f, 0.0f, 1.0f, 850.0f, -60.999969482421875f, 75.0f, 75.0f, KeyCode.F1, false, ButtonPattern.Vehicle, "lineLight09", "", 0f, 0.0f, 0.0f, "f1"),
				new MobileButtonSpec("f2", 0.0f, 1.0f, 0.0f, 1.0f, 950.0f, -61.0f, 75.0f, 75.0f, KeyCode.F2, false, ButtonPattern.Vehicle, "lineLight09", "", 0f, 0.0f, 0.0f, "f2"),
				new MobileButtonSpec("f3", 0.0f, 1.0f, 0.0f, 1.0f, 1049.9998779296875f, -60.999969482421875f, 75.0f, 75.0f, KeyCode.F3, false, ButtonPattern.Vehicle, "lineLight09", "", 0f, 0.0f, 0.0f, "f3"),
				new MobileButtonSpec("f4", 0.0f, 1.0f, 0.0f, 1.0f, 1150.0f, -60.999969482421875f, 75.0f, 75.0f, KeyCode.F4, false, ButtonPattern.Vehicle, "lineLight09", "", 0f, 0.0f, 0.0f, "f4"),
				new MobileButtonSpec("f5", 0.0f, 1.0f, 0.0f, 1.0f, 1249.9998779296875f, -60.999969482421875f, 75.0f, 75.0f, KeyCode.F5, false, ButtonPattern.Vehicle, "lineLight09", "", 0f, 0.0f, 0.0f, "f5"),
				new MobileButtonSpec("f6", 0.0f, 1.0f, 0.0f, 1.0f, 1350.0f, -61.0001220703125f, 75.0f, 75.0f, KeyCode.F6, false, ButtonPattern.Vehicle, "lineLight09", "", 0f, 0.0f, 0.0f, "f6"),
				new MobileButtonSpec("Chat", 1.0f, 1.0f, 1.0f, 1.0f, -74.0f, -173.0f, 75.0f, 75.0f, KeyCode.Return, false, ButtonPattern.None, "lineLight09", "write", 0f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("A", 0.0f, 0.0f, 0.0f, 0.0f, 300.20001220703125f, 256.0f, 169.1999969482422f, 169.20001220703125f, KeyCode.A, false, ButtonPattern.Vehicle, "lineLight09", "crosshairDownFree", -90f, -100.0f, -100.0f, ""),
				new MobileButtonSpec("D", 0.0f, 0.0f, 0.0f, 0.0f, 670.5f, 256.0f, 169.1999969482422f, 169.20001220703125f, KeyCode.D, false, ButtonPattern.Vehicle, "lineLight09", "crosshairDownFree", 90f, -100.0f, -100.0f, ""),
				new MobileButtonSpec("W", 1.0f, 0.0f, 1.0f, 0.0f, -523.0f, 451.74005126953125f, 169.199951171875f, 169.20001220703125f, KeyCode.W, true, ButtonPattern.Vehicle, "lineLight09", "crosshairDownFree", -90f, -100.0f, -100.0f, ""),
				new MobileButtonSpec("S", 1.0f, 0.0f, 1.0f, 0.0f, -523.0f, 229.0f, 169.199951171875f, 169.20001220703125f, KeyCode.S, true, ButtonPattern.Vehicle, "lineLight09", "crosshairDownFree", 90f, -100.0f, -100.0f, ""),
				new MobileButtonSpec("ExitCar", 1.0f, 1.0f, 1.0f, 1.0f, -211.0f, -73.0f, 100.0f, 100.0f, KeyCode.F, false, ButtonPattern.Vehicle, "lineLight09", "back", 0f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("HUD", 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 75.0f, 75.0f, KeyCode.Home, false, ButtonPattern.None, "lineLight09", "", 45f, -30.0f, -30.0f, ""),
				new MobileButtonSpec("Light", 0.0f, 1.0f, 0.0f, 1.0f, 53.0f, -161.3000030517578f, 75.0f, 75.0f, KeyCode.N, false, ButtonPattern.None, "lineLight09", "", 0f, -30.0f, -30.0f, ""),
				new MobileButtonSpec("H", 1.0f, 1.0f, 1.0f, 1.0f, -74.0f, -275.20001220703125f, 75.0f, 75.0f, KeyCode.H, true, ButtonPattern.None, "lineLight09", "Tab", -45f, -30.0f, -30.0f, ""),
				new MobileButtonSpec("Fire1", 0.0f, 1.0f, 0.0f, 1.0f, 246.0f, -284.0f, 125.0f, 125.0f, KeyCode.Mouse0, true, ButtonPattern.None, "lineLight09", "pvp", 135f, -40.0f, -40.0f, ""),
				new MobileButtonSpec("B", 0.0f, 1.0f, 0.0f, 1.0f, 256.00006103515625f, -62.5f, 75.0f, 75.0f, KeyCode.B, false, ButtonPattern.None, "lineLight09", "", 0f, 0.0f, 0.0f, "B"),
			};

			Font builtinFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

			foreach (var spec in specs)
			{
				CreateSingleMobileButton(parent, spec, hudColor, builtinFont);
			}
		}

		private void CreateSingleMobileButton(Transform parent, MobileButtonSpec spec, Color hudColor, Font font)
		{
			GameObject btnGo = CreateUIElement(spec.name, parent,
				new Vector2(spec.aminX, spec.aminY), new Vector2(spec.amaxX, spec.amaxY),
				new Vector2(spec.posX, spec.posY), new Vector2(spec.sizeW, spec.sizeH), new Vector2(0.5f, 0.5f));

			Image btnImg = btnGo.AddComponent<Image>();
			btnImg.sprite = MobileSpriteProvider.GetSprite(spec.bgSprite);
			btnImg.color = hudColor;
			btnImg.raycastTarget = true;

			SpecialButton sb = btnGo.AddComponent<SpecialButton>();
			sb.keySimulate = spec.keyCode;
			sb.lookSimulate = spec.lookSimulate;

			CanvasGroup cg = btnGo.AddComponent<CanvasGroup>();
			sb.m_CanvasGroup = cg;

			Patterns pat = btnGo.AddComponent<Patterns>();
			pat.m_ButtonPattern = spec.pattern;
			pat.m_CanvasGroup = cg;
			pat.m_SpecialButton = sb;
			sb.m_Patterns = pat;

			MobileUIButton mb = btnGo.AddComponent<MobileUIButton>();
			mb.rectTransform = btnGo.GetComponent<RectTransform>();

			// Child Icon GameObject
			GameObject iconGo = new GameObject("Icon");
			iconGo.transform.SetParent(btnGo.transform, false);
			RectTransform iconRt = iconGo.AddComponent<RectTransform>();
			iconRt.anchorMin = Vector2.zero;
			iconRt.anchorMax = Vector2.one;
			iconRt.anchoredPosition = Vector2.zero;
			iconRt.sizeDelta = new Vector2(spec.iconSizeOffsetW, spec.iconSizeOffsetH);
			iconRt.localRotation = Quaternion.Euler(0f, 0f, spec.iconRotZ);

			if (!string.IsNullOrEmpty(spec.iconSprite))
			{
				Image iconImg = iconGo.AddComponent<Image>();
				iconImg.sprite = MobileSpriteProvider.GetSprite(spec.iconSprite);
				iconImg.color = hudColor;
				iconImg.raycastTarget = false;
			}
			else if (!string.IsNullOrEmpty(spec.label))
			{
				Text txt = iconGo.AddComponent<Text>();
				txt.text = spec.label;
				txt.font = font;
				txt.fontSize = 32;
				txt.alignment = TextAnchor.MiddleCenter;
				txt.color = hudColor;
				txt.raycastTarget = false;

				Outline outline = iconGo.AddComponent<Outline>();
				outline.effectColor = new Color(0f, 0f, 0f, 0.5f);
				outline.effectDistance = new Vector2(1f, -1f);
				outline.useGraphicAlpha = true;
			}
		}

		private void CreateSizeSlider(Transform parent, ControllerManager mgr)
		{
			GameObject sliderGo = CreateUIElement("SliderSizeRegulator_048gr3gerbr5535g", parent,
				new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160f, 20f), new Vector2(0.5f, 0.5f));

			Image bgImg = sliderGo.AddComponent<Image>();
			bgImg.sprite = MobileSpriteProvider.GetSprite("sleekImageFree");
			bgImg.color = new Color(1f, 1f, 1f, 0.39215686f);

			Slider slider = sliderGo.AddComponent<Slider>();
			slider.minValue = 0.5f;
			slider.maxValue = 2.0f;
			slider.value = 1.0f;

			GameObject fillArea = CreateUIElement("Fill Area", sliderGo.transform,
				Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, 0f), new Vector2(0.5f, 0.5f));
			GameObject fill = CreateUIElement("Fill", fillArea.transform,
				Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
			Image fillImg = fill.AddComponent<Image>();
			fillImg.sprite = MobileSpriteProvider.GetSprite("sleekFlipFree");
			fillImg.color = new Color(1f, 1f, 1f, 0.6f);
			slider.fillRect = fill.GetComponent<RectTransform>();

			GameObject handleArea = CreateUIElement("Handle Slide Area", sliderGo.transform,
				Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, 0f), new Vector2(0.5f, 0.5f));
			GameObject handleGo = CreateUIElement("Handle", handleArea.transform,
				new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(20f, 0f), new Vector2(0.5f, 0.5f));
			Image handleImg = handleGo.AddComponent<Image>();
			handleImg.sprite = MobileSpriteProvider.GetSprite("normal");
			handleImg.color = Color.white;
			slider.handleRect = handleGo.GetComponent<RectTransform>();
			slider.targetGraphic = handleImg;

			mgr.resizeSlider = slider;
			sliderGo.SetActive(false);
		}

		private static GameObject CreateUIElement(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Vector2 pivot)
		{
			GameObject go = new GameObject(name);
			go.transform.SetParent(parent, false);
			RectTransform rt = go.AddComponent<RectTransform>();
			rt.anchorMin = anchorMin;
			rt.anchorMax = anchorMax;
			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
			rt.pivot = pivot;
			return go;
		}
	}
}
