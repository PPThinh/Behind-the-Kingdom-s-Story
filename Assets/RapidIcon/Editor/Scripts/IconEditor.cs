using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;
using JetBrains.Annotations;
using System.Reflection;
using System.Security.Permissions;
using TMPro.SpriteAssetUtilities;

namespace RapidIconUIC
{
	[Serializable]
	public class IconEditor
	{
		RapidIconWindow window;
		public AssetGrid assetGrid;
		int currentIconIndex;
		public Icon currentIcon;
		bool resizeChk;
		Vector2 previewScrollPos, controlsScrollPos;
		public Vector2Int renderResolution, renderSize;
		GUIStyle renderStyle, scrollStyle;
		int zoomScaleIndex;
		float[] zoomScales = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f, 3f, 1f };
		string[] zoomScalesStrings = new string[] { "25%", "50%", "75%", "100%", "125%", "150%", "200%", "300%", "Scale to Fit ( num %)" };
		public int resMultiplyerIndex;
		public float[] resMultiplyers = new float[] { 0.25f, 0.5f, 1f };
		string[] resMultiplyersStrings = new string[] { "Quarter", "Half", "Full" };
		bool setAlphaIsTransparency;
		public Texture2D previewBackgroundImage, scrollAreaBackgroundImage, scaleLinkOnImage, scaleLinkOffImage, separatorTex, animWarnImage;
		Vector2 previewAreaSize;
		int zoomFitByWidthHeight; //0: height, 1: width
		int tab;
		string[] tabNames = new string[] {"Object", "Camera", "Lighting","Animation", "Post-Processing", "Export" };
		bool updateFlag, updateAllFlag;
		bool linkScale;
		bool replaceAll;
		float sepWidth;
		string lastPresetPath;
		public bool fullscreen;
		float fullWidth;
		float oldMinWidth;
		bool undoHold;
		Rect previewRect;

		MaterialEditor materialEditor;
		Material mat;
		ReorderableList reorderableList;

		DraggableSeparator previewDraggableSeparator;

		public IconEditor(AssetGrid grid, RapidIconWindow w)
		{
			assetGrid = grid;
			renderResolution = new Vector2Int(256, 256);
			renderSize = new Vector2Int(256, 256);
			renderStyle = new GUIStyle();
			renderStyle.stretchWidth = true;
			renderStyle.stretchHeight = true;
			renderStyle.stretchHeight = true;
			renderStyle.stretchWidth = true;
			zoomScaleIndex = 8;
			CheckAndSetWindow(w);
			resMultiplyerIndex = 2;

			scrollAreaBackgroundImage = Utils.CreateColourTexture(4, 4, new Color32(50, 50, 50, 255));

			if (EditorGUIUtility.isProSkin)
				separatorTex = Utils.CreateColourTexture(2, 2, new Color32(31, 31, 31, 255));
			else
				separatorTex = Utils.CreateColourTexture(2, 2, new Color32(153, 153, 153, 255));
			previewBackgroundImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/previewGrid.png");
			scaleLinkOnImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/linkOn.png");
			scaleLinkOffImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/linkOff.png");
			animWarnImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(assetGrid.rapidIconRootFolder + "Editor/UI/animWarning.png");
			previewDraggableSeparator = new DraggableSeparator(SeparatorTypes.Horizontal);
			linkScale = true;
			replaceAll = false;
			mat = new Material(Shader.Find("RapidIcon/ImgShader"));
			materialEditor = (MaterialEditor)Editor.CreateEditor(mat);

			List<Material> blankList = new List<Material>();
			reorderableList = new ReorderableList(blankList, typeof(Material), true, true, true, true);
			reorderableList.drawElementCallback = DrawListItems;
			reorderableList.drawHeaderCallback = DrawHeader;
			reorderableList.onSelectCallback = SelectShader;
			reorderableList.onAddCallback = AddShader;
			reorderableList.onRemoveCallback = RemoveShader;
			reorderableList.onReorderCallback = ShadersReorded;
			undoHold = false;

			Undo.undoRedoPerformed += OnUndo;
		}

		public void Draw(float width, RapidIconWindow w)
		{
			CheckAndSetWindow(w);

			if (!sceneChangeUpdate)
			{
				foreach (Icon icon in assetGrid.objectIcons.Values)
				{
					if (icon.postProcessingMaterials.Count > 0 && icon.postProcessingMaterials[0] == null)
					{
						if (icon.saveData)
							icon.LoadMatInfo();
						else
						{
							ObjectPathPair obj = new ObjectPathPair(icon.assetObject, icon.assetPath);
							Icon newIcon = assetGrid.CreateIcon(obj);
							CopyIconSettings(newIcon, currentIcon, -1);
							UpdateIcon(currentIcon);
						}

						updateAllFlag = true;
						sceneChangeUpdate = true;

					}
				}
			}

			if (assetGrid.selectedIcons.Count > 0)
			{
				currentIconIndex = Mathf.Clamp(currentIconIndex, 0, assetGrid.selectedIcons.Count - 1);
				currentIcon = assetGrid.selectedIcons[currentIconIndex];

				if (currentIcon.assetObject == null)
				{
					currentIcon.deleted = true;
					assetGrid.selectedIcons.Remove(currentIcon);

					if (assetGrid.selectedIcons.Count > 0)
					{
						if (currentIconIndex > assetGrid.selectedIcons.Count - 1)
							currentIconIndex = assetGrid.selectedIcons.Count - 1;

						currentIcon = assetGrid.selectedIcons[currentIconIndex];
					}
					else
						return;
				}

				//Create an area to prevent buggy behaviour when moving left separator
				Rect r = new Rect(window.rightSeparator.rect);
				if (fullscreen)
				{
					r.x = 0;
					r.width = window.position.width;
				}
				else
				{
					r.width = width;
				}
				GUILayout.BeginArea(r);

				GUILayout.BeginVertical();
				GUI.enabled = assetGrid.selectedIcons.Count > 0;

				if (updateFlag)
				{
					UpdateIcon(currentIcon);
					updateFlag = false;
				}

				if(updateAllFlag)
				{
					assetGrid.RefreshAllIcons();
					updateAllFlag = false;
				}
				CheckCurrentIconRender();

				DrawPreview();
				GUILayout.Space(2);
				DrawPreviewResAndZoom();
				previewDraggableSeparator.Draw(100, window.position.height - 100, window);

				GUILayout.Space(8);

				DrawIconSelecter();
				tab = GUILayout.Toolbar(tab, tabNames);

				sepWidth = width - 50;
				DrawTabs(width);
				CheckMouseMovement();
				GUI.enabled = true;
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
			{
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("No Icons Selected");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			}

		}

		public void SaveData()
		{
			previewDraggableSeparator.SaveData("RapidIconSepPosPreview");

			IconData iconData = new IconData();

			foreach (KeyValuePair<UnityEngine.Object, Icon> icon in assetGrid.objectIcons)
			{
				if (icon.Value.saveData)
					iconData.icons.Add(icon.Value);
			}

			Utils.SaveIconData(iconData);

			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconPreviewResIdx", resMultiplyerIndex);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconPreviewZoomIdx", zoomScaleIndex);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconEditorTab", tab);

			if (fullscreen)
			{
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosX", window.position.xMax - (fullWidth + window.position.width));
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosY", window.position.y);
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowWidth", fullWidth + window.position.width);
			}
			else
			{
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosX", -1);
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowPosY", -1);
				EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconWindowWidth", -1);
			}
		}

		public void LoadData()
		{
			IconData iconData = new IconData();
			iconData = Utils.LoadIconData();

			assetGrid.objectIcons = new Dictionary<UnityEngine.Object, Icon>();
			if (iconData != null)
			{
				foreach (Icon icon in iconData.icons)
				{
					if (icon.assetObject != null)
						assetGrid.objectIcons.Add(icon.assetObject, icon);
				}
			}

			string version = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconVersion", "1.0");
			if (version == "1.0")
			{
				if (iconData != null)
				{
					Debug.Log("RapidIcon previously version 1.0, updating icons for 1.1");
					Utils.Update1_0To1_1(iconData.icons);
				}
			}

			version = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconVersion", "1.0");
			if (version == "1.1")
			{
				if (iconData != null)
				{
					Debug.Log("RapidIcon previously version 1.1, updating icons for 1.2");
					Utils.Update1_1To1_2(iconData.icons);
				}
			}

			version = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconVersion", "1.0");
			if (version == "1.2.1")
			{
				if (iconData != null)
				{
					Debug.Log("RapidIcon previously version 1.2.1, updating icons for 1.3");
					Utils.Update1_2_1To1_3(iconData.icons);
				}
			}

			previewDraggableSeparator.LoadData("RapidIconSepPosPreview", 650);

			resMultiplyerIndex = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconPreviewResIdx", -1);
			zoomScaleIndex = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconPreviewZoomIdx", -1);

			if (resMultiplyerIndex == -1)
				resMultiplyerIndex = 2;

			if (zoomScaleIndex == -1)
				zoomScaleIndex = 8;

			tab = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconEditorTab", 0);
		}

		void OnUndo()
		{
			updateFlag = true;

			foreach (Icon icon in assetGrid.objectIcons.Values)
				icon.LoadMatInfo();
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		void ExportIcon(Icon icon, bool inBatchExport)
		{
			if (!Directory.Exists(icon.exportFolderPath))
				Directory.CreateDirectory(icon.exportFolderPath);

			string fileName = icon.exportFolderPath + icon.exportPrefix + icon.exportName + icon.exportSuffix + ".png";

			if (System.IO.File.Exists(fileName) && !replaceAll)
			{
				int result = 1;

				if (inBatchExport)
					result = EditorUtility.DisplayDialogComplex("Replace File?", fileName + " already exists, do you want to replace it?", "Replace", "Skip", "Replace All");
				else
					result = EditorUtility.DisplayDialog("Replace File?", fileName + " already exists, do you want to replace it?", "Replace", "Cancel") ? 0 : 1;

				if (result == 1)
					return;
				else if(result == 2)
				{
					replaceAll = true;
				}
			}

			if(AssetDatabase.IsValidFolder(icon.exportFolderPath))
				AssetDatabase.DeleteAsset(fileName);

			Texture2D exportRender = Utils.RenderIcon(icon, icon.exportResolution.x, icon.exportResolution.y);
			byte[] bytes = exportRender.EncodeToPNG();

			File.WriteAllBytes(fileName, bytes);
		}

		void FinishExportIcon(List<Icon> icons)
		{
			AssetDatabase.Refresh();
			foreach (Icon icon in icons)
			{
				string fileName = icon.exportFolderPath + icon.exportPrefix + icon.exportName + icon.exportSuffix + ".png";

				if (AssetDatabase.IsValidFolder(icon.exportFolderPath.Substring(0, icon.exportFolderPath.Length - 1)))
				{
					TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fileName);
					if (textureImporter != null)
					{
						textureImporter.alphaIsTransparency = true;
						textureImporter.npotScale = TextureImporterNPOTScale.None;
						textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
						textureImporter.SaveAndReimport();
						textureImporter.filterMode = icon.filterMode;
					}
				}
			}
			AssetDatabase.Refresh();
		}
		void FinishExportIcon(Icon icon)
		{
			AssetDatabase.Refresh();
			string fileName = icon.exportFolderPath + icon.exportPrefix + icon.exportName + icon.exportSuffix + ".png";

			if (AssetDatabase.IsValidFolder(icon.exportFolderPath.Substring(0, icon.exportFolderPath.Length - 1)))
			{
				TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fileName);
				textureImporter.alphaIsTransparency = true;
				textureImporter.npotScale = TextureImporterNPOTScale.None;
				textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
				textureImporter.SaveAndReimport();
				textureImporter.filterMode = icon.filterMode;
			}
			AssetDatabase.Refresh();
		}

		void UpdateIcon(Icon icon)
		{
			renderResolution = Utils.MutiplyVector2IntByFloat(currentIcon.exportResolution, resMultiplyers[resMultiplyerIndex]);
			icon.Update(renderResolution, new Vector2Int(128, (int)(((float)renderResolution.y / (float)renderResolution.x) * 128)));
		}

		void DrawIconSelecter()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("<", GUILayout.Width(100)) && currentIconIndex > 0)
				currentIconIndex--;

			int idx = 0;
			string[] iconNames = new string[assetGrid.selectedIcons.Count];
			foreach (Icon icon in assetGrid.selectedIcons)
				iconNames[idx++] = (icon.assetName);

			currentIconIndex = EditorGUILayout.Popup(currentIconIndex, iconNames);
			if (GUILayout.Button(">", GUILayout.Width(100)) && currentIconIndex < assetGrid.selectedIcons.Count - 1)
				currentIconIndex++;
			GUILayout.EndHorizontal();

			// Placeholder
			/*GUILayout.BeginHorizontal();
			if (GUILayout.Button("<") && currentIconIndex > 0)
				currentIconIndex--;
			GUILayout.Label(currentIconIndex + ": " + currentIcon.assetShortenedName);
			if (GUILayout.Button(">") && currentIconIndex < assetGrid.selectedIcons.Count - 1)
				currentIconIndex++;
			GUILayout.EndHorizontal();*/
		}

		void CheckMouseMovement()
		{
			if (!currentIcon.doAnimation || Application.isPlaying)
			{
				if (Event.current.button == 1 && Event.current.type == EventType.Layout)
				{
					if (previewRect.Contains(Event.current.mousePosition))
					{
						if (!undoHold)
						{
							Undo.RecordObject(window, "Camera Rotation");
							undoHold = true;
						}

						Quaternion camTurnAngle = Quaternion.AngleAxis(Event.current.delta.x * 0.2f, Vector3.up);

						Vector3 axis = Quaternion.LookRotation(currentIcon.cameraTarget - currentIcon.cameraPosition) * Vector3.right;
						camTurnAngle *= Quaternion.AngleAxis(Event.current.delta.y * 0.2f, axis);
						currentIcon.cameraPosition = camTurnAngle * currentIcon.cameraPosition;

						currentIcon.saveData = true;
						updateFlag = true;
						window.Repaint();
					}
				}
				else if (Event.current.type == EventType.ScrollWheel)
				{

					if (previewRect.Contains(Event.current.mousePosition))
					{
						if (!undoHold)
						{
							Undo.RecordObject(window, "Camera Zoom");
							undoHold = true;
						}

						if (currentIcon.cameraOrtho)
						{
							if (Mathf.Sign(Event.current.delta.y) == 1)
								currentIcon.camerasScaleFactor /= 1.1f;
							else
								currentIcon.camerasScaleFactor *= 1.1f;
						}
						else
						{
							if (Mathf.Sign(Event.current.delta.y) == 1)
								currentIcon.cameraPosition *= 1.1f;
							else
								currentIcon.cameraPosition /= 1.1f;
						}


						currentIcon.saveData = true;
						updateFlag = true;
						window.Repaint();
					}
				}
				else if (undoHold)
					undoHold = false;
			}
		}

		void DrawPreview()
		{
			if(scrollStyle == null)
			{
				scrollStyle = new GUIStyle(GUI.skin.scrollView);
				scrollStyle.margin.left = 1;
				scrollStyle.normal.background = scrollAreaBackgroundImage;
			}

			previewScrollPos = GUILayout.BeginScrollView(previewScrollPos, scrollStyle, GUILayout.Height(previewDraggableSeparator.value));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			//renderStyle.normal.background = currentIcon.fullRender;
			//renderStyle.fixedHeight = renderSize.y - 12;
			//renderStyle.fixedWidth = renderSize.x - 12;
			Rect renderRect = GUILayoutUtility.GetRect(renderSize.x, renderSize.y);
			renderRect.size -= new Vector2(12, 12);
			renderRect.center += new Vector2(6, 6);
			GUI.DrawTextureWithTexCoords(renderRect, previewBackgroundImage, new Rect(0, 0, renderRect.width / 32, renderRect.height / 32));
			//GUI.SelectionGrid(renderRect, 0, new GUIContent[] { new GUIContent("") }, 1, renderStyle);
			GUI.DrawTexture(renderRect, currentIcon.doAnimation && !Application.isPlaying ? animWarnImage : currentIcon.fullRender);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndScrollView();
			if (Event.current.type == EventType.Repaint)
			{
				previewRect = GUILayoutUtility.GetLastRect();
				previewAreaSize = previewRect.size;
				Vector2 delta = currentIcon.exportResolution - previewRect.size;

				delta.x /= currentIcon.exportResolution.x;
				delta.y /= currentIcon.exportResolution.y;
				if (delta.x > 0 && delta.y > 0)
				{
					if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
						zoomFitByWidthHeight = 0;
					else
						zoomFitByWidthHeight = 1;
				}
				else if (delta.x <= 0 && delta.y <= 0)
				{
					if (Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
						zoomFitByWidthHeight = 1;
					else
						zoomFitByWidthHeight = 0;
				}
				else if (delta.y > 0)
					zoomFitByWidthHeight = 0;
				else if (delta.x > 0)
					zoomFitByWidthHeight = 1;
			}
		}

		void DrawPreviewResAndZoom()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			switch (zoomFitByWidthHeight)
			{
				case 0:
					zoomScales[8] = previewAreaSize.y / (float)(currentIcon.exportResolution.y);
					break;
				case 1:
					zoomScales[8] = previewAreaSize.x / (float)(currentIcon.exportResolution.x);
					break;
			}
			if (assetGrid.selectedIcons.Count == 0)
				zoomScales[8] = 1;
			string s = "Scale to Fit (" + (100f * (float)zoomScales[8]).ToString("f1") + "%)";
			zoomScalesStrings[8] = s;

			GUILayout.Label("Preview Resolution", GUILayout.Width(115));
			resMultiplyerIndex = EditorGUILayout.Popup(resMultiplyerIndex, resMultiplyersStrings, GUILayout.Width(70));
			renderResolution = Utils.MutiplyVector2IntByFloat(currentIcon.exportResolution, resMultiplyers[resMultiplyerIndex]);

			if (EditorGUI.EndChangeCheck())
			{
				currentIcon = assetGrid.selectedIcons[currentIconIndex];
				if (currentIcon != null)
				{
					updateFlag = true;
				}
			}

			GUILayout.Space(32);

			GUILayout.Label("Zoom", GUILayout.Width(40));
			zoomScaleIndex = EditorGUILayout.Popup(zoomScaleIndex, zoomScalesStrings, GUILayout.Width(150));
			renderSize = Utils.MutiplyVector2IntByFloat(currentIcon.exportResolution, zoomScales[zoomScaleIndex]);
			GUILayout.EndHorizontal();
		}

		void CheckCurrentIconRender()
		{
			if (!currentIcon.fullRender)
			{
				renderResolution = Utils.MutiplyVector2IntByFloat(currentIcon.exportResolution, resMultiplyers[resMultiplyerIndex]);
				currentIcon.fullRender = Utils.RenderIcon(currentIcon, renderResolution.x, renderResolution.y);
				currentIcon.fullRender.hideFlags = HideFlags.DontSave;
			}
		}

		bool sceneChangeUpdate = false;
		void DrawTabs(float width)
		{
			controlsScrollPos = GUILayout.BeginScrollView(controlsScrollPos, GUILayout.Height(window.position.height - previewDraggableSeparator.value - 98));

			GUI.enabled = Application.isPlaying || !currentIcon.doAnimation;
			if(!GUI.enabled)
			{
				GUI.enabled = true;
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUIStyle warning = new GUIStyle(GUI.skin.label);
				warning.normal.textColor = Color.red;
				GUILayout.Label("Enter play mode or disable animations to edit this icon", warning);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUI.enabled = false;
			}
			switch (tab)
			{
				case 0:
					DrawObjectControls();
					break;
				case 1:
					DrawCameraControls();
					break;
				case 2:
					DrawLightingControls();
					break;
				case 3:
					DrawAnimationControls();
					break;
				case 4:
					DrawPostProcessingControls();
					break;
				case 5:
					DrawExportControls();
					break;
			}
			GUI.enabled = true;

			if (tab < 5 && !(tab==3 && !HasAnimations(currentIcon)))
			{
				GUILayout.Space(8);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Apply to All Selected Icons", GUILayout.Width(200)))
					ApplyToAllSelectedIcons(tab);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(4);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset This Icon", GUILayout.Width(200)))
			{
				if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to reset all settings for the current icon? This cannot be undone.", "Reset", "Cancel"))
				{
					ObjectPathPair obj = new ObjectPathPair(currentIcon.assetObject, currentIcon.assetPath);
					Icon newIcon = assetGrid.CreateIcon(obj);
					CopyIconSettings(newIcon, currentIcon, -1);
					UpdateIcon(currentIcon);
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset All Selected Icons", GUILayout.Width(200)))
			{
				if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to reset all settings for the currently selected icons? This cannot be undone.", "Reset", "Cancel"))
				{
					int index = 0;
					foreach (Icon ic in assetGrid.selectedIcons)
					{
						EditorUtility.DisplayProgressBar("Resetting Icons (" + index + " / " + (assetGrid.selectedIcons.Count) + ")", ic.assetName, (float)(index++) / (float)assetGrid.selectedIcons.Count);

						ObjectPathPair obj = new ObjectPathPair(ic.assetObject, ic.assetPath);
						Icon newIcon = assetGrid.CreateIcon(obj);
						CopyIconSettings(newIcon, ic, -1);
						UpdateIcon(ic);
					}
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();

			if (GUILayout.Button(fullscreen ? "<" : ">", GUILayout.Width(20)))
			{
				fullscreen = !fullscreen;
				if (fullscreen)
				{
					fullWidth = window.position.width - width;
					oldMinWidth = window.minSize.x;
					window.minSize = new Vector2(400, window.minSize.y);
					window.position = new Rect(window.position.xMax - width, window.position.y, width, window.position.height);
				}
				else
				{
					window.position = new Rect(window.position.xMax - (fullWidth + window.position.width), window.position.y, fullWidth + window.position.width, window.position.height);
					window.minSize = new Vector2(oldMinWidth, window.minSize.y);
				}
			}

		}

		void ApplyToAllSelectedIcons(int tab)
		{
			int result = 1;
			result = EditorUtility.DisplayDialogComplex("Apply to All Selected Icons", "Would you like to apply only " + tabNames[tab] + " settings, or all settings?", tabNames[tab] + " Settings Only", "Cancel", "All Settings");

			if (result == 1)
				return;
			else
			{
				int index = 1;

				Undo.RegisterCompleteObjectUndo(window, "Apply to all icons");

				foreach(Icon icon in assetGrid.selectedIcons)
				{
					if (icon != currentIcon)
					{
						EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (assetGrid.selectedIcons.Count - 1)));
						bool doUpdate = (result == 2) | !(tab == 5 && icon.exportResolution == currentIcon.exportResolution);

						CopyIconSettings(currentIcon, icon, result == 2 ? -1 : tab);
						icon.SaveMatInfo();
						icon.saveData = true;

						if(doUpdate)
							UpdateIcon(icon);
					}
				}

				if (tab == 4 || tab == -1)
				{
					Editor.DestroyImmediate(materialEditor);
					if (reorderableList.list != null && reorderableList.list.Count > 0)
						materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)reorderableList.list[reorderableList.index]);
				}

				EditorUtility.ClearProgressBar();
			}
		}

		void CopyIconSettings(Icon from, Icon to, int tab)
		{
			if (tab == 0 || tab == -1)
			{
				to.objectPosition = from.objectPosition;
				to.objectRotation = from.objectRotation;
				to.objectScale = from.objectScale;
			}

			if (tab == 1 || tab == -1)
			{
				to.cameraPosition = from.cameraPosition;
				to.cameraRotation = from.cameraRotation;
				to.cameraOrtho = from.cameraOrtho;
				to.cameraFov = from.cameraFov;
				to.cameraSize = from.cameraSize;
				to.camerasScaleFactor = from.camerasScaleFactor;
				//to.cameraMode = from.cameraMode;
				to.cameraTarget = from.cameraTarget;
			}

			if (tab == 2 || tab == -1)
			{
				to.ambientLightColour = from.ambientLightColour;
				to.lightColour = from.lightColour;
				to.lightDir = from.lightDir;
				to.lightIntensity = from.lightIntensity;
			}

			if(tab == 3 || tab == -1)
			{
				to.LoadAnimations();
				if (!from.doAnimation || HasAnimations(to))
				{
					to.doAnimation = from.doAnimation;
					to.animationLayer = from.animationLayer;
					to.animationStateHash = from.animationStateHash;
					to.animationStateIdx = from.animationStateIdx;
					to.animationOffset = from.animationOffset;
				}
			}
			
			if (tab == 4 || tab == -1)
			{
				foreach (Material mat in to.postProcessingMaterials)
					Editor.DestroyImmediate(mat);

				to.postProcessingMaterials.Clear();

				foreach (Material mat in from.postProcessingMaterials)
				{
					Material newMat = new Material(mat);
					to.postProcessingMaterials.Add(newMat);
					to.materialDisplayNames.Add(newMat, from.materialDisplayNames[mat]);
					to.materialToggles.Add(newMat, from.materialToggles[mat]);
				}

				to.doFixAlphaEdges = from.doFixAlphaEdges;
				to.filterMode = from.filterMode;
			}

			if (tab == 5 || tab == -1)
			{
				to.exportResolution = from.exportResolution;
				to.exportFolderPath = from.exportFolderPath;
				to.exportPrefix = from.exportPrefix;
				to.exportSuffix = from.exportSuffix;
			}

			to.saveData = true;
		}

		void DrawObjectControls()
		{
			EditorGUI.BeginChangeCheck();

			Vector3 tmpObjPos = EditorGUILayout.Vector3Field("Position", currentIcon.objectPosition);
			Rect posR = GUILayoutUtility.GetLastRect();
			posR.x += 50;
			posR.height = 18;
			posR.width = 50;

			if (GUI.Button(posR, "Auto"))
			{
				Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);
				GameObject go = (GameObject)currentIcon.assetObject;

				Vector3 prefabPos = go.transform.position;
				go.transform.position = Vector3.zero;

				MeshRenderer mr = go.GetComponent<MeshRenderer>();
				if (mr != null)
					bounds.Encapsulate(mr.bounds);

				Utils.EncapsulateChildBounds(go.transform, ref bounds);

				go.transform.position = prefabPos;

				tmpObjPos = -bounds.center;
			}
			posR.x += 50;
			posR.width = 150;
			if (GUI.Button(posR, "Auto All Selected Icons"))
			{
				int index = 1;
				foreach (Icon icon in assetGrid.selectedIcons)
				{
					EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (assetGrid.selectedIcons.Count - 1)));
					Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);
					GameObject go = (GameObject)icon.assetObject;

					Vector3 prefabPos = go.transform.position;
					go.transform.position = Vector3.zero;

					MeshRenderer mr = go.GetComponent<MeshRenderer>();
					if (mr != null)
						bounds.Encapsulate(mr.bounds);

					Utils.EncapsulateChildBounds(go.transform, ref bounds);

					go.transform.position = prefabPos;

					if (icon == currentIcon)
						tmpObjPos = -bounds.center;
					else
						icon.objectPosition = -bounds.center;

					UpdateIcon(icon);
				}
				EditorUtility.ClearProgressBar();
			}

			Vector3 tmpObjRot = EditorGUILayout.Vector3Field("Rotation", currentIcon.objectRotation);
			Vector3 tmpObjScale = EditorGUILayout.Vector3Field("Scale", currentIcon.objectScale);

			Rect r = GUILayoutUtility.GetLastRect();
			r.position += new Vector2(40, 0);
			if (GUI.Button(r, linkScale ? scaleLinkOnImage : scaleLinkOffImage, GUIStyle.none))
			{
				linkScale = !linkScale;
			}

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(window, "Edit Icon Object");
				currentIcon.objectPosition = tmpObjPos;
				currentIcon.objectRotation = tmpObjRot;

				if (!linkScale)
				{
					currentIcon.objectScale = tmpObjScale;
				}
				else
				{
					if (tmpObjScale.x != currentIcon.objectScale.x)
					{
						currentIcon.objectScale = tmpObjScale.x * Vector3.one;
					}
					else if (tmpObjScale.y != currentIcon.objectScale.y)
					{
						currentIcon.objectScale = tmpObjScale.y * Vector3.one;
					}
					else if (tmpObjScale.z != currentIcon.objectScale.z)
					{
						currentIcon.objectScale = tmpObjScale.z * Vector3.one;
					}
				}

				currentIcon.saveData = true;
				updateFlag = true;
			}
		}

		void DrawCameraControls()
		{
			EditorGUI.BeginChangeCheck();
			Vector3 tmpCamPos = EditorGUILayout.Vector3Field("Position", currentIcon.cameraPosition);

			GUILayout.Space(8);
			float tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			//int tmpCamMode = EditorGUILayout.Popup("Rotation", currentIcon.cameraMode, new string[] { "Specify Angles", "Target Point" });
			//Vector3 tmpCamRot = currentIcon.cameraRotation;
			Vector3 tmpCamTgt = currentIcon.cameraTarget;
			/*if (currentIcon.cameraMode == 0)
			{
				tmpCamRot = EditorGUILayout.Vector3Field("", currentIcon.cameraRotation);
			}
			else if (currentIcon.cameraMode == 1)
			{*/
			tmpCamTgt = EditorGUILayout.Vector3Field("Point of Focus", currentIcon.cameraTarget);
			//}

			GUILayout.Space(8);
			string[] listOptions = { "Perspective", "Orthographic" };
			EditorGUIUtility.labelWidth = 80;
			int tmpOrtho = EditorGUILayout.Popup("Projection", currentIcon.cameraOrtho ? 1 : 0, listOptions);

			float tmpSizeFov;
			float tmpScaleFac = currentIcon.camerasScaleFactor;
			if (tmpOrtho == 1)
			{
				GUILayout.BeginHorizontal();
				tmpSizeFov = EditorGUILayout.FloatField("Size", currentIcon.cameraSize);

				if (GUILayout.Button("Auto", GUILayout.Width(50)))
				{
					Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);
					GameObject go = (GameObject)currentIcon.assetObject;

					Vector3 prefabPos = go.transform.position;
					go.transform.position = Vector3.zero;

					MeshRenderer mr = go.GetComponent<MeshRenderer>();
					if (mr != null)
						bounds.Encapsulate(mr.bounds);

					Utils.EncapsulateChildBounds(go.transform, ref bounds);
					go.transform.position = prefabPos;

					Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), 1.1f * Vector3.one);
					Vector3 corner = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
					corner = trs * corner;
					Vector2 b2 = HandleUtility.WorldToGUIPoint(corner);
					tmpSizeFov = b2.magnitude;
				}
				if (GUILayout.Button("Auto All Selected Icons", GUILayout.Width(150)))
				{
					int index = 1;
					foreach (Icon icon in assetGrid.selectedIcons)
					{
						EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (assetGrid.selectedIcons.Count - 1)));
						Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);
						GameObject go = (GameObject)icon.assetObject;

						Vector3 prefabPos = go.transform.position;
						go.transform.position = Vector3.zero;

						MeshRenderer mr = go.GetComponent<MeshRenderer>();
						if (mr != null)
							bounds.Encapsulate(mr.bounds);

						Utils.EncapsulateChildBounds(go.transform, ref bounds);
						go.transform.position = prefabPos;

						Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), 1.1f * Vector3.one);
						Vector3 corner = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
						corner = trs * corner;
						Vector2 b2 = HandleUtility.WorldToGUIPoint(corner);
						if (icon == currentIcon)
							tmpSizeFov = b2.magnitude;
						else
							icon.cameraSize = b2.magnitude;
						UpdateIcon(icon);
					}
					EditorUtility.ClearProgressBar();
				}

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				tmpScaleFac = EditorGUILayout.FloatField("Scale Factor", currentIcon.camerasScaleFactor);
				if (GUILayout.Button("Apply to All Selected Icons", GUILayout.Width(170)))
				{
					Undo.RegisterCompleteObjectUndo(window, "Apply scale factor to all icons");

					int index = 1;
					foreach (Icon icon in assetGrid.selectedIcons)
					{
						if (icon != currentIcon)
						{
							EditorUtility.DisplayProgressBar("Updating Icons (" + index + "/" + (assetGrid.selectedIcons.Count - 1) + ")", icon.assetPath, ((float)index++ / (assetGrid.selectedIcons.Count - 1)));
							icon.camerasScaleFactor = currentIcon.camerasScaleFactor;
							icon.saveData = true;
							UpdateIcon(icon);
						}
					}
					EditorUtility.ClearProgressBar();
				}
				GUILayout.EndHorizontal();
			}
			else
				tmpSizeFov = EditorGUILayout.FloatField("Field of View", currentIcon.cameraFov);

			EditorGUIUtility.labelWidth = tmpWdith;
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(window, "Edit Icon Camera");

				//currentIcon.cameraMode = tmpCamMode;
				currentIcon.cameraPosition = tmpCamPos;
				//currentIcon.cameraRotation = tmpCamRot;
				currentIcon.cameraTarget = tmpCamTgt;
				currentIcon.cameraOrtho = tmpOrtho == 1 ? true : false;


				if (currentIcon.cameraOrtho)
				{
					currentIcon.cameraSize = tmpSizeFov;
					currentIcon.camerasScaleFactor = tmpScaleFac;
				}
				else
					currentIcon.cameraFov = tmpSizeFov;

				currentIcon.saveData = true;
				updateFlag = true;
			}
		}

		void DrawLightingControls()
		{
			EditorGUI.BeginChangeCheck();
			Color tmpAmbLight = EditorGUILayout.ColorField("Ambient Light Colour", currentIcon.ambientLightColour);

			EditorGUILayout.Space(4);
			EditorGUILayout.LabelField("Directional Light");

			Color tmpLightColour = EditorGUILayout.ColorField("Colour", currentIcon.lightColour);
			Vector3 tmpLightDir = EditorGUILayout.Vector3Field("Rotation", currentIcon.lightDir);
			float tmpLightIntensity = EditorGUILayout.FloatField("Intensity", currentIcon.lightIntensity);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(window, "Edit Icon Lighting");

				currentIcon.ambientLightColour = tmpAmbLight;

				currentIcon.lightColour = tmpLightColour;
				currentIcon.lightDir = tmpLightDir;
				currentIcon.lightIntensity = tmpLightIntensity;

				currentIcon.saveData = true;
				updateFlag = true;
			}
		}

		void DrawAnimationControls()
		{
			EditorGUI.BeginChangeCheck();

			if (!HasAnimations(currentIcon))
			{
				currentIcon.doAnimation = false;
				updateFlag = true;
				GUILayout.Label("This object does not have an Animator component");

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Reload Animations", GUILayout.Width(200)))
				{
					currentIcon.LoadAnimations();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				return;
			}

			bool tmp = GUI.enabled;
			GUI.enabled = true;
			currentIcon.doAnimation = GUILayout.Toggle(currentIcon.doAnimation, "Enable Animations");
			GUI.enabled = tmp;

			if (currentIcon.doAnimation)
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Reload Animations", GUILayout.Width(200)))
				{
					currentIcon.LoadAnimations();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				currentIcon.animationLayer = EditorGUILayout.Popup("Animation Layer", currentIcon.animationLayer, currentIcon.animLayerNames);

				currentIcon.animationStateIdx = Mathf.Clamp(currentIcon.animationStateIdx, 0, currentIcon.animStateNames[currentIcon.animationLayer].Length - 1);
				currentIcon.animationStateIdx = EditorGUILayout.Popup("Animation State", currentIcon.animationStateIdx, currentIcon.animStateNames[currentIcon.animationLayer]);

				currentIcon.animationStateHash = currentIcon.animStateHashes[currentIcon.animationLayer][currentIcon.animationStateIdx];

				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Animation Offset", GUILayout.Width(150));
				currentIcon.animationOffset = GUILayout.HorizontalSlider(currentIcon.animationOffset, 0, 1);
				GUILayout.EndHorizontal();
				GUILayout.Space(8);
				GUILayout.EndVertical();
			}

			if (EditorGUI.EndChangeCheck())
			{
				updateFlag = true;
				currentIcon.saveData = true;
			}
		}

		bool HasAnimations(Icon icon)
		{
			if (icon.animLayerNames == null || icon.animLayerNames.Length == 0)
				return false;

			if (icon.animStateNames == null || icon.animStateNames.Length == 0)
				return false;

			return true;
		}

		void DrawPostProcessingControls()
		{
			EditorGUI.BeginChangeCheck();
			FilterMode tmpFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", currentIcon.filterMode);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(window, "1 Filter Mode");
				currentIcon.filterMode = tmpFilterMode;
				updateFlag = true;
			}

			reorderableList.list = currentIcon.postProcessingMaterials;
			reorderableList.index = (int)Mathf.Clamp(reorderableList.index, 0, reorderableList.list.Count - 1);

			GUILayout.Space(2);

			reorderableList.DoLayoutList();

			EditorGUI.BeginChangeCheck();
			if (reorderableList.list.Count > 0)
			{
				string shaderName = currentIcon.postProcessingMaterials[reorderableList.index].shader.name;


				GUILayout.Label("Shader Settings (" + shaderName + ")");
				GUILayout.BeginVertical(GUI.skin.box);
				if (shaderName != "RapidIcon/ObjectRender")
				{
					UnityEngine.Object[] obj = new UnityEngine.Object[1];
					obj[0] = (UnityEngine.Object)reorderableList.list[reorderableList.index];
					MaterialProperty[] props = MaterialEditor.GetMaterialProperties(obj);
					
					if (materialEditor.customShaderGUI == null)
					{
						foreach (MaterialProperty prop in props)
						{
							if (prop.name != "_MainTex" && prop.flags != MaterialProperty.PropFlags.HideInInspector)
							{
								materialEditor.ShaderProperty(prop, prop.displayName);
							}
						}
					}
					else
					{
						List<MaterialProperty> list = new List<MaterialProperty>(props);
						foreach (MaterialProperty prop in list)
						{
							if (prop.name == "_MainTex")
							{
								list.Remove(prop);
								break;
							}
						}
						props = list.ToArray();

						materialEditor.customShaderGUI.OnGUI(materialEditor, props);
					}

				}
				else
				{

					EditorGUI.BeginChangeCheck();
					bool tmpDoFixAlphaEdges = GUILayout.Toggle(!currentIcon.doFixAlphaEdges, "Premultiplied Alpha");

					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(window, "Toggle Icon Premultiplied Alpha");
						currentIcon.doFixAlphaEdges = !tmpDoFixAlphaEdges;
					}
				}
				GUILayout.EndVertical();
			}

			if (EditorGUI.EndChangeCheck())
			{
				currentIcon.SaveMatInfo();
				updateFlag = true;
			}
		}

		void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Shaders");

			float sw2 = sepWidth;
			if (fullscreen)
				sw2 += fullWidth;

			rect.width = 100;
			rect.x = sw2 - 154;
			if(GUI.Button(rect, "Save Preset"))
			{
				string savePath = EditorUtility.SaveFilePanel("Save Preset", lastPresetPath == "" ? Application.dataPath : lastPresetPath, "PostProcessingPreset", "rippp");
				if (savePath != "")
				{
					currentIcon.SaveMatInfo();
					string data = JsonUtility.ToJson(currentIcon);
					int pos = data.IndexOf("\"matInfo\":");
					data = "{" + data.Substring(pos);

					File.WriteAllBytes(savePath, System.Text.Encoding.ASCII.GetBytes(data));
					lastPresetPath = savePath;
				}
			}

			rect.x += 102;
			if(GUI.Button(rect, "Load Preset"))
			{
				string openPath = EditorUtility.OpenFilePanel("Open Preset", lastPresetPath == "" ? Application.dataPath: lastPresetPath, "rippp");
				if (openPath != "")
				{
					Byte[] bytes = File.ReadAllBytes(openPath);
					lastPresetPath = openPath;
					string data = System.Text.Encoding.ASCII.GetString(bytes);

					Icon tmp = JsonUtility.FromJson<Icon>(data);

					currentIcon.saveData = true;
					currentIcon.matInfo = new List<Icon.MaterialInfo>(tmp.matInfo);
					currentIcon.LoadMatInfo();
					UpdateIcon(currentIcon);
					reorderableList.list = currentIcon.postProcessingMaterials;
					reorderableList.index = 0;
					Editor.DestroyImmediate(materialEditor);
					materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)reorderableList.list[reorderableList.index]);
				}
			}
		}

		void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
		{
			if (index >= 0 && index < reorderableList.list.Count)
			{
				float sw2 = sepWidth;
				if (fullscreen)
					sw2 += fullWidth;

				float[] widths = new float[] { 16, (sw2 - 150) / 2, (sw2 + 100) / 2, 80 };

				if (currentIcon.materialToggles == null)
				{
					window.Close();
					return;
				}

				bool tmp = GUI.enabled;
				if(tmp)
					GUI.enabled = currentIcon.materialToggles[currentIcon.postProcessingMaterials[index]];
				currentIcon.materialDisplayNames[currentIcon.postProcessingMaterials[index]] = EditorGUI.TextField(new Rect(rect.x+widths[0]+4, rect.y + 3, widths[1], EditorGUIUtility.singleLineHeight), currentIcon.materialDisplayNames[currentIcon.postProcessingMaterials[index]]);

				GUI.enabled = tmp;
				EditorGUI.BeginChangeCheck();
					currentIcon.materialToggles[currentIcon.postProcessingMaterials[index]] = EditorGUI.Toggle(new Rect(rect.x, rect.y + 3, widths[0], EditorGUIUtility.singleLineHeight), currentIcon.materialToggles[currentIcon.postProcessingMaterials[index]]);

				if(tmp)
					GUI.enabled = currentIcon.materialToggles[currentIcon.postProcessingMaterials[index]];

				currentIcon.postProcessingMaterials[index].shader = (Shader)EditorGUI.ObjectField(new Rect(rect.x + widths[0] + widths[1] + 8, rect.y + 3, widths[2], EditorGUIUtility.singleLineHeight), currentIcon.postProcessingMaterials[index].shader, typeof(Shader), true);

				GUI.enabled = tmp;

				if (EditorGUI.EndChangeCheck())
				{
					updateFlag = true;
					Editor.DestroyImmediate(materialEditor);
					materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)reorderableList.list[reorderableList.index]);
				}
			}
		}

		void SelectShader(ReorderableList l)
		{
			Editor.DestroyImmediate(materialEditor);
			materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)reorderableList.list[reorderableList.index]);
		}

		void AddShader(ReorderableList l)
		{
			Undo.RecordObject(window, "Add New Shader");

			Material m = new Material(Shader.Find("RapidIcon/ObjectRender"));
			
			l.list.Add(m);
			l.index = l.list.Count - 1;

			currentIcon.materialDisplayNames.Add(m, "Shader " + l.list.Count);
			currentIcon.materialToggles.Add(m, true);

			Editor.DestroyImmediate(materialEditor);
			materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)reorderableList.list[reorderableList.index]);

			currentIcon.saveData = true;
			updateFlag = true;
		}

		void RemoveShader(ReorderableList l)
		{
			Undo.RecordObject(window, "Remove Shader");
			l.list.Remove(l.list[l.index]);
			l.index = (int)Mathf.Clamp(l.index, 0, l.list.Count - 1);

			Editor.DestroyImmediate(materialEditor);
			if(l.list.Count > 0)
				materialEditor = (MaterialEditor)Editor.CreateEditor((UnityEngine.Object)reorderableList.list[reorderableList.index]);

			currentIcon.saveData = true;
			updateFlag = true;
		}

		void ShadersReorded(ReorderableList l)
		{
			currentIcon.saveData = true;
			updateFlag = true;
		}

		void DrawExportControls()
		{
			EditorGUI.BeginChangeCheck();

			Vector2Int tmpExportRes = EditorGUILayout.Vector2IntField("Export Resolution", currentIcon.exportResolution);
			tmpExportRes.x = (int)Mathf.Clamp(tmpExportRes.x, 8, 2048);
			tmpExportRes.y = (int)Mathf.Clamp(tmpExportRes.y, 8, 2048);

			GUILayout.Space(2);

			GUILayout.BeginHorizontal();
			if (currentIcon.exportFolderPath[currentIcon.exportFolderPath.Length - 1] != '/')
				currentIcon.exportFolderPath += "/";

			float tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			currentIcon.exportFolderPath = EditorGUILayout.TextField("Export Folder", currentIcon.exportFolderPath);


			string tmpFolder = currentIcon.exportFolderPath;
			tmpFolder.Replace(" ", "");
			if(tmpFolder == "")
			{
				string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIconWindow")[0]).Split('/');
				string rapidIconRootFolder = "";
				for (int i = 0; i < split.Length - 3; i++)
					rapidIconRootFolder += split[i] + "/";

				currentIcon.exportFolderPath = rapidIconRootFolder + "Exports/";
			}

			EditorGUIUtility.labelWidth = tmpWdith;
			if (GUILayout.Button("Browse", GUILayout.Width(100)))
			{
				string folder = EditorUtility.OpenFolderPanel("Export Folder", currentIcon.exportFolderPath, "");
				if (folder != "")
				{
					string dataPath = Application.dataPath;
					dataPath = dataPath.Substring(0, dataPath.LastIndexOf('/') + 1);
					folder = folder.Replace(dataPath, "");
					currentIcon.exportFolderPath = folder;
				}
			}
			GUILayout.EndHorizontal();

			tmpWdith = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 80;
			string exportPrefix = EditorGUILayout.TextField("Export Prefix", currentIcon.exportPrefix);
			string exportSuffix = EditorGUILayout.TextField("Export Suffix", currentIcon.exportSuffix);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(window, "Edit Icon Export Resolution");
				bool doUpdate = tmpExportRes != currentIcon.exportResolution;

				currentIcon.exportResolution = tmpExportRes;
				currentIcon.exportPrefix = exportPrefix;
				currentIcon.exportSuffix = exportSuffix;
				currentIcon.saveData = true;
			

				if (doUpdate && currentIcon.exportResolution.x > 0 && currentIcon.exportResolution.y > 0)
				{
					updateFlag = true;
				}
			}
					   
			if (GUILayout.Button("Apply to All Selected Icons"))
				ApplyToAllSelectedIcons(5);

			GUILayout.Space(12);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			Rect r = GUILayoutUtility.GetRect(sepWidth, 1);
			if(separatorTex == null)
			{
				if (EditorGUIUtility.isProSkin)
					separatorTex = Utils.CreateColourTexture(2, 2, new Color32(31, 31, 31, 255));
				else
					separatorTex = Utils.CreateColourTexture(2, 2, new Color32(153, 153, 153, 255));
			}
			GUI.DrawTexture(r, separatorTex);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space(12);

			EditorGUI.BeginChangeCheck();
			string exportName = EditorGUILayout.TextField("Export Name", currentIcon.exportName);

			EditorGUIUtility.labelWidth = tmpWdith;
			GUILayout.Label("Full Export Name: " + currentIcon.exportPrefix + currentIcon.exportName + currentIcon.exportSuffix + ".png");
			if (EditorGUI.EndChangeCheck())
			{
				if (exportName.Length > 0)
				{
					currentIcon.exportName = exportName;
					currentIcon.saveData = true;
				}
			}

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Export Icon", GUILayout.Width(160)))
			{
				ExportIcon(currentIcon, false);
				FinishExportIcon(currentIcon);
				replaceAll = false;
			}
			else if (GUILayout.Button("Export Selected Icons", GUILayout.Width(160)))
			{
				AssetDatabase.StartAssetEditing();
				int index = 1;
				List<string> exportPaths = new List<string>();
				bool warningDisplayed = false;
				foreach (Icon icon in assetGrid.selectedIcons)
				{
					if (!icon.doAnimation || Application.isPlaying)
					{
						EditorUtility.DisplayProgressBar("Exporting Icons (" + index + "/" + assetGrid.selectedIcons.Count + ")", icon.assetPath, ((float)index++ / assetGrid.selectedIcons.Count));
						ExportIcon(icon, assetGrid.selectedIcons.Count > 1 ? true : false);
					}
					else if(!warningDisplayed)
					{
						EditorUtility.DisplayDialog("Warning", "Icons with animations enabled will be skipped, enter play mode to export these icons", "OK");
						warningDisplayed = true;
					}
				}
				EditorUtility.ClearProgressBar();

				AssetDatabase.StopAssetEditing();
				FinishExportIcon(assetGrid.selectedIcons);

				replaceAll = false;
			}
			GUILayout.EndHorizontal();

		}

	}
}