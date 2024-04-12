using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace RapidIconUIC
{
	public struct ObjectPathPair
	{
		public ObjectPathPair(UnityEngine.Object obj, string pth)
		{
			UnityEngine_object = obj;
			path = pth;
		}

		public UnityEngine.Object UnityEngine_object;
		public string path;
	};

	[Serializable]
	public class AssetGrid
	{
		public bool showAllSubFolderObjects;
		Vector2 scrollPosition;
		AssetList assetList;
		int previewSize;
		public string rapidIconRootFolder;
		GUIStyle gridStyle, gridLabelStyle;
		Texture2D[] assetSelectionTextures;
		RapidIconWindow window;
		public bool assetGridFocused;
		Rect rect;
		int lastSelectedIconIndex;
		int selectionMinIndex, selectionMaxIndex;
		string lastSelectedIndividualFolder;
		bool iconsRefreshed;

		List<ObjectPathPair> objectsLoadedFromSelectedFolders;
		public Dictionary<UnityEngine.Object, Icon> objectIcons;
		public Dictionary<string, Icon> sortedIconsByPath;
		public List<Icon> selectedIcons;
		public List<Icon> visibleIcons;

		int filterIdx;
		string[] filters = new string[] { "t:model t:prefab", "t:prefab", "t:model" };
		string[] filterNames = new string[] { "Prefabs & Models", "Prefabs Only", "Models Only" };

		/*public List<Icon> selectedIcons;	
		Dictionary<UnityEngine.Object, string> loadedObjectPaths;
		public Dictionary<UnityEngine.Object, Icon> icons;
		public Dictionary<UnityEngine.Object, Icon> loadedIcons;
		Dictionary<string, Icon> iconsFromPath;*/


		public AssetGrid(AssetList assets)
		{
			showAllSubFolderObjects = false;
			assetList = assets;
			objectsLoadedFromSelectedFolders = new List<ObjectPathPair>();
			previewSize = 128;
			objectIcons = new Dictionary<UnityEngine.Object, Icon>();
			sortedIconsByPath = new Dictionary<string, Icon>();
			selectedIcons = new List<Icon>();
			gridStyle = new GUIStyle();
			gridStyle.fixedHeight = previewSize;
			gridStyle.fixedWidth = previewSize;
			gridStyle.margin.bottom = 16 + (int)EditorGUIUtility.singleLineHeight + 2;
			gridStyle.margin.left = 16;
			gridStyle.alignment = TextAnchor.MiddleCenter;
			gridLabelStyle = new GUIStyle(gridStyle);
			gridLabelStyle.margin.bottom = 16 + previewSize + 2;
			gridLabelStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
			gridLabelStyle.alignment = TextAnchor.MiddleCenter;

			filterIdx = 0;

			if (EditorGUIUtility.isProSkin)
				gridLabelStyle.normal.textColor = new Color32(192, 192, 192, 255);
			else
				gridLabelStyle.normal.textColor = Color.black;

			assetSelectionTextures = new Texture2D[5];
			string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIconWindow")[0]).Split('/');
			rapidIconRootFolder = "";
			for (int i = 0; i < split.Length - 3; i++)
				rapidIconRootFolder += split[i] + "/";

			assetSelectionTextures[0] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/deselectedAsset.png");
			assetSelectionTextures[1] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetActiveDark.png");
			assetSelectionTextures[2] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetInactiveDark.png");
			assetSelectionTextures[3] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetActiveLight.png");
			assetSelectionTextures[4] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/selectedAssetInactiveLight.png");
			assetSelectionTextures[0].hideFlags = HideFlags.DontSave;
			assetSelectionTextures[1].hideFlags = HideFlags.DontSave;
			assetSelectionTextures[2].hideFlags = HideFlags.DontSave;
			assetList.lastNumberOfSelected = -1;

			lastSelectedIconIndex = -1;
			selectionMinIndex = int.MaxValue;
			selectionMaxIndex = -1;

			iconsRefreshed = true;
		}

		public void Draw(float width, RapidIconWindow w)
		{
			CheckAndSetWindow(w);

			GUILayout.BeginVertical(GUILayout.Width(width));

			GUILayout.Space(4);

			GUILayout.BeginHorizontal();
			GUILayout.Space(8);
			if (GUILayout.Button("Refresh"))
				ReloadObjects();
			if (GUILayout.Button("Filter: " + filterNames[filterIdx]))
			{
				filterIdx++;
				if (filterIdx == 3)
					filterIdx = 0;

				ReloadObjects();
			}
			GUILayout.FlexibleSpace();
			//int count = sortedIconsByPath.Count;
			//GUILayout.Label(count.ToString() + (count == 1 ? " Object" : " Objects") + " Found");
			GUILayout.EndHorizontal();

			scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

			//objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			//CreateIcons();

			if (!iconsRefreshed && EditorApplication.timeSinceStartup > 15)
			{
				RefreshAllIcons();
				iconsRefreshed = true;
			}

			DrawIcons(width);

			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			if (Event.current.type == EventType.Repaint)
				rect = new Rect(GUILayoutUtility.GetLastRect());

			CheckFocus(rect);
		}

		public void SaveData()
		{
			string selectedAssetsString = "";
			foreach (KeyValuePair<UnityEngine.Object, Icon> icon in objectIcons)
			{
				selectedAssetsString += "|-A-|" + icon.Value.assetPath + "|-S-|" + icon.Value.selected;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconSelectedAssets", selectedAssetsString);
			EditorPrefs.SetFloat(PlayerSettings.productName + "RapidIconAssetGridScroll", scrollPosition.y);
			EditorPrefs.SetBool(PlayerSettings.productName + "RapidIconIconsRefreshed", iconsRefreshed);
			EditorPrefs.SetInt(PlayerSettings.productName + "RapidIconFilterIdx", filterIdx);
		}

		public void LoadData()
		{
			iconsRefreshed = EditorPrefs.GetBool(PlayerSettings.productName + "RapidIconIconsRefreshed");
			objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			CreateIcons();

			string selectedAssetsString = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconSelectedAssets");
			string[] splitAssets = selectedAssetsString.Split(new string[] { "|-A-|" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string s in splitAssets)
			{
				string[] splitS = s.Split(new string[] { "|-S-|" }, StringSplitOptions.RemoveEmptyEntries);
				string assetPath = splitS[0];
				if (splitS[1] == "True")
				{
					Icon icon = GetIconFromPath(assetPath);
					if (icon != null)
					{
						icon.selected = true;
						selectedIcons.Add(GetIconFromPath(assetPath));
					}
				}
			}

			scrollPosition = new Vector2(0, EditorPrefs.GetFloat(PlayerSettings.productName + "RapidIconAssetGridScroll"));
			filterIdx = EditorPrefs.GetInt(PlayerSettings.productName + "RapidIconFilterIdx", 0);
		}

		void ReloadObjects()
		{
			EditorUtility.UnloadUnusedAssetsImmediate();

			objectsLoadedFromSelectedFolders = LoadObjectsInSelectedFolders();
			CreateIcons();

			sortedIconsByPath.Clear();
			foreach (ObjectPathPair loadedObject in objectsLoadedFromSelectedFolders)
			{
				Icon icon = objectIcons[loadedObject.UnityEngine_object];
				sortedIconsByPath.Add(icon.assetPath, icon);
			}

			sortedIconsByPath = SortIconsByFolder(sortedIconsByPath);
			assetList.lastNumberOfSelected = assetList.selectedFolders.Count;
			lastSelectedIndividualFolder = assetList.selectedFolders[0];
		}

		public void RefreshAllIcons()
		{
			int index = 1;
			foreach (Icon icon in objectIcons.Values)
			{
				EditorUtility.DisplayProgressBar("Updating Icons (" + index++ + "/" + objectIcons.Count + ")", icon.assetPath, (float)(index)/(float)(objectIcons.Count));
				Vector2Int renderResolution = Utils.MutiplyVector2IntByFloat(icon.exportResolution, window.iconEditor.resMultiplyers[window.iconEditor.resMultiplyerIndex]);
				icon.Update(renderResolution, new Vector2Int(128, (int)(((float)renderResolution.y / (float)renderResolution.x) * 128)));
			}
			EditorUtility.ClearProgressBar();
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		void CheckArrowKeys(List<Icon> icons, int gridXIcons)
		{
			//Check if a key is pressed
			if (assetGridFocused && Event.current.isKey && Event.current.type != EventType.KeyUp)
			{
				//----Right arrow key pressed----
				if (Event.current.keyCode == KeyCode.RightArrow && lastSelectedIconIndex < icons.Count - 1)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						foreach (Icon icon in icons)
							icon.selected = false;
						selectedIcons.Clear();

						icons[lastSelectedIconIndex + 1].selected = true;
						selectedIcons.Add(icons[lastSelectedIconIndex + 1]);
					}
					else
					{
						if (!icons[lastSelectedIconIndex + 1].selected)
						{
							icons[lastSelectedIconIndex + 1].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex + 1]);
						}
						else
						{
							icons[lastSelectedIconIndex].selected = false;
							selectedIcons.Remove(icons[lastSelectedIconIndex]);
						}

					}
					lastSelectedIconIndex++;
				}
				//----Left arrow key pressed----
				else if (Event.current.keyCode == KeyCode.LeftArrow && lastSelectedIconIndex > 0)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						foreach (Icon icon in icons)
							icon.selected = false;
						selectedIcons.Clear();

						icons[lastSelectedIconIndex - 1].selected = true;
						selectedIcons.Add(icons[lastSelectedIconIndex - 1]);
					}
					else
					{
						if (!icons[lastSelectedIconIndex - 1].selected)
						{
							icons[lastSelectedIconIndex - 1].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex - 1]);
						}
						else
						{
							icons[lastSelectedIconIndex].selected = false;
							selectedIcons.Remove(icons[lastSelectedIconIndex]);
						}
					}
					lastSelectedIconIndex--;
				}
				//----Down arrow key pressed----
				else if (Event.current.keyCode == KeyCode.DownArrow)
				{
					if (lastSelectedIconIndex < icons.Count - gridXIcons)
					{
						if (!Event.current.shift && !Event.current.control)
						{
							foreach (Icon icon in icons)
								icon.selected = false;
							selectedIcons.Clear();

							icons[lastSelectedIconIndex + gridXIcons].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex + gridXIcons]);
						}
						else
						{
							if (!icons[lastSelectedIconIndex + gridXIcons].selected)
							{
								icons[lastSelectedIconIndex + gridXIcons].selected = true;
								selectedIcons.Add(icons[lastSelectedIconIndex + gridXIcons]);
								for (int i = lastSelectedIconIndex; i < lastSelectedIconIndex + gridXIcons; i++)
								{
									icons[i].selected = true;
									selectedIcons.Add(icons[i]);
								}
							}
							else
							{
								icons[lastSelectedIconIndex].selected = false;
								selectedIcons.Remove(icons[lastSelectedIconIndex]);
								for (int i = lastSelectedIconIndex; i < lastSelectedIconIndex + gridXIcons; i++)
								{
									icons[i].selected = false;
									selectedIcons.Remove(icons[i]);
								}
							}

						}
						lastSelectedIconIndex += gridXIcons;
					}
					else if (lastSelectedIconIndex < Mathf.Floor((float)icons.Count/gridXIcons)*gridXIcons)
					{
						if (!Event.current.shift && !Event.current.control)
						{
							foreach (Icon icon in icons)
								icon.selected = false;
							selectedIcons.Clear();

							icons[icons.Count - 1].selected = true;
							selectedIcons.Add(icons[icons.Count - 1]);
						}
						else
						{
							if (!icons[icons.Count - 1].selected)
							{
								icons[icons.Count - 1].selected = true;
								selectedIcons.Add(icons[icons.Count - 1]);
								for (int i = lastSelectedIconIndex; i < icons.Count; i++)
								{
									icons[i].selected = true;
									selectedIcons.Add(icons[i]);
								}
							}
							else
							{
								icons[lastSelectedIconIndex].selected = false;
								selectedIcons.Remove(icons[lastSelectedIconIndex]);
								for (int i = lastSelectedIconIndex; i < icons.Count - 1; i++)
								{
									icons[i].selected = false;
									selectedIcons.Remove(icons[i]);
								}
							}

						}
						lastSelectedIconIndex = icons.Count - 1;
					}
				}
				//----Up arrow key pressed----
				else if (Event.current.keyCode == KeyCode.UpArrow && lastSelectedIconIndex >= gridXIcons)
				{
					if (!Event.current.shift && !Event.current.control)
					{
						foreach (Icon icon in icons)
							icon.selected = false;
						selectedIcons.Clear();

						icons[lastSelectedIconIndex - gridXIcons].selected = true;
						selectedIcons.Add(icons[lastSelectedIconIndex - gridXIcons]);
					}
					else
					{
						if (!icons[lastSelectedIconIndex - gridXIcons].selected)
						{
							icons[lastSelectedIconIndex - gridXIcons].selected = true;
							selectedIcons.Add(icons[lastSelectedIconIndex - gridXIcons]);
							for (int i = lastSelectedIconIndex; i > lastSelectedIconIndex - gridXIcons; i--)
							{
								icons[i].selected = true;
								selectedIcons.Add(icons[i]);
							}
						}
						else
						{
							icons[lastSelectedIconIndex].selected = false;
							selectedIcons.Remove(icons[lastSelectedIconIndex]);
							for (int i = lastSelectedIconIndex; i > lastSelectedIconIndex - gridXIcons; i--)
							{
								icons[i].selected = false;
								selectedIcons.Remove(icons[i]);
							}
						}

					}
					lastSelectedIconIndex -= gridXIcons;
				}
				else if (Event.current.keyCode == KeyCode.A && Event.current.modifiers == EventModifiers.Control)
				{
					selectedIcons.Clear();
					foreach (KeyValuePair<string, Icon> icon in sortedIconsByPath)
					{
						if (assetList.selectedFolders.Contains(icon.Value.folderPath))
						{
							icon.Value.selected = true;
							selectedIcons.Add(icon.Value);
						}
					}
				}
			}
		}

		List<ObjectPathPair> LoadObjectsInSelectedFolders()
		{
			string[] assetGUIDs = AssetDatabase.FindAssets(filters[filterIdx], assetList.selectedFolders.ToArray());
			string[] assetPaths = new string[assetGUIDs.Length];
			for (int i = 0; i < assetGUIDs.Length; i++)
				assetPaths[i] = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

			List<ObjectPathPair> loadedObjectPathPairs = new List<ObjectPathPair>();
			foreach (string assetPath in assetPaths)
			{

				string[] split = assetPath.Split('/');
				string folderPath = "";
				for (int i = 0; i < split.Length - 1; i++)
					folderPath += split[i] + (i < split.Length - 2 ? "/" : "");


				if (assetList.selectedFolders.Contains(folderPath))
				{
					ObjectPathPair objectPathPair = new ObjectPathPair();

					UnityEngine.Object o = AssetDatabase.LoadMainAssetAtPath(assetPath);
					objectPathPair.UnityEngine_object = o;
					objectPathPair.path = assetPath;
					loadedObjectPathPairs.Add(objectPathPair);
				}
			}
			return loadedObjectPathPairs;
		}

		void CreateIcons()
		{
			int index = 1;
			foreach (ObjectPathPair loadedObject in objectsLoadedFromSelectedFolders)
			{
				if (!objectIcons.ContainsKey(loadedObject.UnityEngine_object))
				{
					EditorUtility.DisplayProgressBar("Generating Icon Previews (" + index + " / " + (objectsLoadedFromSelectedFolders.Count) + ")", loadedObject.path, (float)(index++) / (float)objectsLoadedFromSelectedFolders.Count);
					objectIcons.Add(loadedObject.UnityEngine_object, CreateIcon(loadedObject));
				}
				else if (objectIcons[loadedObject.UnityEngine_object].deleted)
				{
					objectIcons[loadedObject.UnityEngine_object].deleted = false;
				}
				else
				{
					Icon icon = objectIcons[loadedObject.UnityEngine_object];

					string currentPath = AssetDatabase.GetAssetPath(loadedObject.UnityEngine_object);
					string savedPath = icon.assetPath;
					//Debug.Log(loadedObject.UnityEngine_object + ": " + savedPath + ", " + currentPath);
					if (savedPath != currentPath)
					{
						Debug.LogWarning("Path updated for " + icon.assetName + " from " + savedPath + " to " + currentPath);
						icon.assetPath = currentPath;

						string[] split;
						split = icon.assetPath.Split('/');
						icon.assetName = split[split.Length - 1];
						if (icon.assetName.Length > 19)
							icon.assetShortenedName = icon.assetName.Substring(0, 16) + "...";
						else
							icon.assetShortenedName = icon.assetName;

						split = icon.assetPath.Split('/');
						icon.folderPath = "";
						for (int i = 0; i < split.Length - 1; i++)
							icon.folderPath += split[i] + (i < split.Length - 2 ? "/" : "");
					}
				}
			}
			EditorUtility.ClearProgressBar();
		}

		void DrawIcons(float gridWidth)
		{
			GUILayout.Space(14);
			GUILayout.BeginHorizontal();
			GUILayout.Space(16);

			List<Texture2D> visibleIconRenders = new List<Texture2D>();
			List<Texture2D> visibleIconSelectionTextures = new List<Texture2D>();
			List<string> visibleIconLabels = new List<string>();
			visibleIcons = new List<Icon>();

			if (sortedIconsByPath.Count != objectsLoadedFromSelectedFolders.Count || assetList.selectedFolders.Count != assetList.lastNumberOfSelected || assetList.selectedFolders[0] != lastSelectedIndividualFolder)
			{
				ReloadObjects();
			}

			foreach (Icon icon in objectIcons.Values)
			{
				if (!assetList.selectedFolders.Contains(icon.folderPath) || (assetList.doSearch && !assetList.searchFolders.Contains(icon.folderPath + "/" + icon.assetName)))
				{
					icon.selected = false;
					if (selectedIcons.Contains(icon))
						selectedIcons.Remove(icon);
				}
			}

			int index = 0;
			foreach (KeyValuePair<string, Icon> icon in sortedIconsByPath)
			{
				if (icon.Value.deleted)
					continue;
				else if (icon.Value.assetObject == null)
				{
					icon.Value.deleted = true;
					icon.Value.assetObject = null;
					//icon.Value.GUIDs[3] = "";
					selectedIcons.Remove(icon.Value);
					continue;
				}

				if (icon.Value.previewRender == null)
				{
					EditorUtility.DisplayProgressBar("Generating Icon Previews (" + index + "/" + (sortedIconsByPath.Count) + ")", icon.Value.assetPath, ((float)index++ / sortedIconsByPath.Count));
					icon.Value.previewRender = Utils.RenderIcon(icon.Value, previewSize, (int)(((float)icon.Value.exportResolution.y / (float)icon.Value.exportResolution.x) * previewSize));
				}

				if (EditorGUIUtility.isProSkin)
					icon.Value.selectionTexture = icon.Value.selected ? (assetGridFocused ? assetSelectionTextures[1] : assetSelectionTextures[2]) : assetSelectionTextures[0];
				else
					icon.Value.selectionTexture = icon.Value.selected ? (assetGridFocused ? assetSelectionTextures[3] : assetSelectionTextures[4]) : assetSelectionTextures[0];

				if (assetList.selectedFolders.Contains(icon.Value.folderPath) && (!assetList.doSearch || assetList.searchFolders.Contains(icon.Value.folderPath + "/" + icon.Value.assetName)))
				{
					visibleIcons.Add(icon.Value);
					visibleIconRenders.Add(icon.Value.doAnimation && !Application.isPlaying ? window.iconEditor.animWarnImage : icon.Value.previewRender);
					visibleIconSelectionTextures.Add(icon.Value.selectionTexture);
					visibleIconLabels.Add(icon.Value.assetShortenedName);
				}
			}
			EditorUtility.ClearProgressBar();
			
			int count = Mathf.FloorToInt((gridWidth - 16) / (previewSize + 16));
			count = Mathf.Min(count, visibleIcons.Count);
			int clicked = GUILayout.SelectionGrid(-1, visibleIconRenders.ToArray(), count, gridStyle, GUILayout.Width(32 + count * (previewSize + 16)));
			Rect r = GUILayoutUtility.GetLastRect();
			r.y += previewSize + 2;

			int labelClicked = GUI.SelectionGrid(r, -1, visibleIconSelectionTextures.ToArray(), count, gridLabelStyle);

			clicked = GUI.SelectionGrid(r, clicked, visibleIconLabels.ToArray(), count, gridLabelStyle);
			if (clicked == -1 && labelClicked != -1)
				clicked = labelClicked;

			GUILayout.Space(16);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			CheckMouseClicks(clicked, visibleIcons);
			CheckArrowKeys(visibleIcons, count);
		}

		void CheckMouseClicks(int clicked, List<Icon> visibleIcons)
		{
			if (clicked >= 0)
			{
				if (!Event.current.control && !Event.current.shift)
				{
					foreach (KeyValuePair<UnityEngine.Object, Icon> icon in objectIcons)
						icon.Value.selected = false;

					visibleIcons[clicked].selected = true;
					visibleIcons[clicked].assetGridIconIndex = clicked;
					selectedIcons.Clear();
					selectionMinIndex = clicked;
					selectionMaxIndex = clicked;
				}
				else if (Event.current.control)
				{
					visibleIcons[clicked].selected = !visibleIcons[clicked].selected;
					visibleIcons[clicked].assetGridIconIndex = clicked;
				}
				else if (Event.current.shift)
				{
					if (selectionMinIndex != -1 && selectionMaxIndex != -1 && clicked >= selectionMinIndex && clicked <= selectionMaxIndex)
					{
						for (int i = selectionMinIndex; i <= selectionMaxIndex; i++)
						{
							visibleIcons[i].selected = false;
							if (selectedIcons.Contains(visibleIcons[i]))
								selectedIcons.Remove(visibleIcons[i]);
						}

						selectionMinIndex = Mathf.Min(lastSelectedIconIndex, clicked);
						selectionMaxIndex = Math.Max(lastSelectedIconIndex, clicked);
					}
					int minI = Mathf.Min(lastSelectedIconIndex, clicked);
					int maxI = Math.Max(lastSelectedIconIndex, clicked);
					if (minI < 0) minI = 0;
					if (maxI < 0) maxI = 0;

					for (int i = minI; i <= maxI; i++)
					{
						visibleIcons[i].selected = true;
						visibleIcons[i].assetGridIconIndex = i;
						if (!selectedIcons.Contains(visibleIcons[i]))
							selectedIcons.Add(visibleIcons[i]);
					}

				}

				if (!Event.current.shift)
				{
					if (visibleIcons[clicked].selected && !selectedIcons.Contains(visibleIcons[clicked]))
						selectedIcons.Add(visibleIcons[clicked]);
					else if (selectedIcons.Contains(visibleIcons[clicked]))
						selectedIcons.Remove(visibleIcons[clicked]);
				}

				if (selectedIcons.Count > 1)
					selectedIcons = selectedIcons.OrderBy(a => a.assetGridIconIndex).ToList();

				selectionMinIndex = Mathf.Min(selectionMinIndex, clicked);
				selectionMaxIndex = Mathf.Max(selectionMaxIndex, clicked);
				lastSelectedIconIndex = clicked;
				assetGridFocused = true;
				window.Repaint();
			}
			else if (Event.current.rawType == EventType.MouseDown && !window.leftSeparator.mouseOver && !window.rightSeparator.mouseOver)
			{
				Vector2 correctMousePos = Event.current.mousePosition + rect.position;
				if (rect.Contains(correctMousePos))
				{
					selectedIcons.Clear();
					foreach (Icon icon in visibleIcons)
						icon.selected = false;
				}
			}
		}

		Dictionary<string, Icon> SortIconsByFolder(Dictionary<string, Icon> data)
		{
			string[] assetPaths = new string[data.Keys.Count];
			data.Keys.CopyTo(assetPaths, 0);

			Dictionary<string, List<string>> folders = new Dictionary<string, List<string>>();
			List<string> folderNames = new List<string>();

			foreach (string assetPath in assetPaths)
			{
				string[] split = assetPath.Split('/');
				string folderPath = "";
				for (int i = 0; i < split.Length - 1; i++)
					folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

				if (!folders.ContainsKey(folderPath))
				{
					folders.Add(folderPath, new List<string>());
					folderNames.Add(folderPath);
				}

				folders[folderPath].Add(assetPath);
			}

			string[] sortedAssetPaths = new string[assetPaths.Length];
			folderNames.Sort();
			int index = 0;
			foreach (string folder in folderNames)
			{
				folders[folder].Sort();
				foreach (string assetPath in folders[folder])
				{
					sortedAssetPaths[index++] = assetPath;
				}
			}

			Dictionary<string, Icon> sortedData = new Dictionary<string, Icon>();
			foreach (string assetPath in sortedAssetPaths)
			{
				sortedData.Add(assetPath, data[assetPath]);
			}

			return sortedData;
		}

		public Icon CreateIcon(ObjectPathPair objectPathPair)
		{
			Icon icon = new Icon(Shader.Find("RapidIcon/ObjectRender"), rapidIconRootFolder, objectPathPair.path);

			icon.assetObject = objectPathPair.UnityEngine_object;
			icon.assetPath = objectPathPair.path;

			GameObject go = (GameObject)icon.assetObject;
			//icon.objectPosition = go.transform.position;
			if (icon.objectPosition.magnitude < 0.0001f)
				icon.objectPosition = Vector3.zero;

			icon.objectRotation = go.transform.eulerAngles;
			if (icon.objectRotation.magnitude < 0.0001f)
				icon.objectRotation = Vector3.zero;

			icon.objectScale = go.transform.localScale;

			Bounds bounds = new Bounds(Vector3.zero, 0.000001f * Vector3.one);

			Vector3 prefabPos = go.transform.position;
			go.transform.position = Vector3.zero;
			MeshRenderer mr = go.GetComponent<MeshRenderer>();
			if (mr != null)
				bounds.Encapsulate(mr.bounds);
			else
			{
				SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
				if (smr != null)
					bounds.Encapsulate(smr.bounds);
			}

			Utils.EncapsulateChildBounds(go.transform, ref bounds);
			icon.objectPosition = -bounds.center;

			go.transform.position = prefabPos;

			Matrix4x4 trs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 45, 0), 1.1f*Vector3.one);
			Vector3 corner = new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			corner = trs * corner;
			Vector2 b2 = HandleUtility.WorldToGUIPoint(corner);
			icon.cameraSize = b2.magnitude;

			icon.previewRender = Utils.RenderIcon(icon, previewSize, (int)(((float)icon.exportResolution.y/ (float)icon.exportResolution.x) * previewSize));
			icon.previewRender.hideFlags = HideFlags.DontSave;

			string[] split;

			split = icon.assetPath.Split('/');
			icon.assetName = split[split.Length - 1];
			if (icon.assetName.Length > 19)
				icon.assetShortenedName = icon.assetName.Substring(0, 16) + "...";
			else
				icon.assetShortenedName = icon.assetName;

			split = icon.assetPath.Split('/');
			icon.folderPath = "";
			for (int i = 0; i < split.Length - 1; i++)
				icon.folderPath += split[i] + (i < split.Length - 2 ? "/" : "");

			icon.exportName = icon.assetName;
			int extensionPos = icon.exportName.LastIndexOf('.');
			icon.exportName = icon.exportName.Substring(0, extensionPos);

			icon.selectionTexture = assetSelectionTextures[1];

			icon.LoadAnimations();

			return icon;
		}

		void CheckFocus(Rect checkRect)
		{
			//Check if last mouse click was in the asset grid rect
			if (Event.current.rawType == EventType.MouseDown)
			{
				assetGridFocused = checkRect.Contains(Event.current.mousePosition);
			}

			//Check the RapidIcon window is in focus
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() != typeof(RapidIconWindow))
				assetGridFocused = false;
		}

		Icon GetIconFromPath(string path)
		{
			foreach(Icon icon in objectIcons.Values)
			{
				if (icon.assetPath == path)
					return icon;
			}

			return null;
		}
	}
}