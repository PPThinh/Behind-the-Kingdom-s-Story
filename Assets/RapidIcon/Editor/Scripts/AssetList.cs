using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace RapidIconUIC
{
	[Serializable]
	public class AssetList
	{
		public List<string> selectedFolders;
		public int lastNumberOfSelected;
		bool foldoutColoursSet, assetListFocused, initialised;
		GUIStyle foldoutStyle, foldoutStyleSelected;
		List<string> visibleFolders;
		Dictionary<string, bool> foldoutStates;
		int arrowSelectedFolder;
		Vector2 scrollPosition;
		string rootFolder;
		Texture2D[] selectionTextures;
		RapidIconWindow window;
		string selectionMinFolder, selectionMaxFolder, firstSelectedFolder, lastSelectedFolder;
		string selectFolder;
		Texture2D[] folderIcons;
		public List<string> searchFolders;
		string searchString;
		public bool doSearch;

		bool foldoutsUpdated;
		
		public AssetList(string appDataPath)
		{
			/*--------------------------------------------------------------------------------
			 * Initialise AssetList
			 *--------------------------------------------------------------------------------*/
			foldoutColoursSet = false;
			assetListFocused = true;
			foldoutStyle = foldoutStyleSelected = new GUIStyle();
			selectedFolders = new List<string>();
			visibleFolders = new List<string>();
			foldoutStates = new Dictionary<string, bool>();
			arrowSelectedFolder = 0;
			string[] rootFolderSplit = appDataPath.Split('/');
			rootFolder = rootFolderSplit[rootFolderSplit.Length - 1];
			selectionTextures = new Texture2D[2];
			searchFolders = new List<string>();
			searchString = "";
			doSearch = false;
			if (EditorGUIUtility.isProSkin)
			{
				selectionTextures[0] = Utils.CreateColourTexture(2, 2, new Color32(44, 93, 135, 255));
				selectionTextures[1] = Utils.CreateColourTexture(2, 2, new Color32(77, 77, 77, 255));
			}
			else
			{
				selectionTextures[0] = Utils.CreateColourTexture(2, 2, new Color32(58, 114, 176, 255));
				selectionTextures[1] = Utils.CreateColourTexture(2, 2, new Color32(174, 174, 174, 255));
			}
			selectionTextures[0].hideFlags = HideFlags.DontSave;
			selectionTextures[1].hideFlags = HideFlags.DontSave;
			string[] split = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("RapidIcon")[0]).Split('/');
			string rapidIconRootFolder = "";
			for (int i = 0; i < split.Length; i++)
				rapidIconRootFolder += split[i] + "/";
			folderIcons = new Texture2D[4];
			folderIcons[0] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/folderIconClosedDark.png");
			folderIcons[1] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/folderIconOpenDark.png");
			folderIcons[2] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/folderIconClosedLight.png");
			folderIcons[3] = (Texture2D)AssetDatabase.LoadMainAssetAtPath(rapidIconRootFolder + "Editor/UI/folderIconOpenLight.png");
			firstSelectedFolder = "";
			lastSelectedFolder = "";
			selectionMinFolder = "";
			selectionMaxFolder = "";
			selectFolder = "";

			foldoutsUpdated = false;
		}

		public void Draw(float width, RapidIconWindow w)
		{
			//----Check variables are set----
			CheckAndSetWindow(w);
			CheckAndSetFoldoutColours();
			CheckArrowKeys();

			//----Search Bar----
			GUILayout.BeginVertical(GUILayout.Width(width));
			EditorGUI.BeginChangeCheck();
			searchString = GUILayout.TextField(searchString);
			if (searchString == "")
			{
				Rect r = GUILayoutUtility.GetLastRect();
				GUI.Label(r, "Search Models and Prefabs");
				searchFolders.Clear();
				doSearch = false;
			}
			else if (EditorGUI.EndChangeCheck())
			{
				doSearch = true;
				searchFolders.Clear();
				string[] results = AssetDatabase.FindAssets(searchString);
				if (results.Length > 0)
				{
					foreach(string guid in results)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(GameObject))
						{
							searchFolders.Add(path);
						}
					}
				}
			}
			

			//----Draw asset list folders----
			visibleFolders.Clear();
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(width));

			if (!foldoutsUpdated)
			{
				UpdateFoldoutStates(rootFolder, true);
				foldoutsUpdated = true;
			}

			DrawAllSubFolders(rootFolder, 0, width);
			if(selectFolder != "")
			{
				SelectFolder(selectFolder, 0);
				selectFolder = "";
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			//----Check if asset list is focused
			CheckFocus(GUILayoutUtility.GetLastRect());
		}

		public void SaveData()
		{
			//----Save all opened folders----
			string openedFoldersString = "";
			foreach (KeyValuePair<string, bool> foldoutState in foldoutStates)
			{
				if (foldoutState.Value)
					openedFoldersString += "|-F-|" + foldoutState.Key;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconOpenedFolders", openedFoldersString);

			//----SAve all selected folders----
			string selectedFoldersString = "";
			foreach (string folder in selectedFolders)
			{
				selectedFoldersString += "|-F-|" + folder;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconSelectedFolders", selectedFoldersString);
		}

		public void LoadData()
		{
			//----Load all opened folders
			string openedFoldersString = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconOpenedFolders");
			string[] openedFoldersSplit = openedFoldersString.Split(new string[] { "|-F-|" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string folder in openedFoldersSplit)
				foldoutStates.Add(folder, true);

			//----Load all selected folders
			string selectedFoldersString = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconSelectedFolders");
			string[] selectedFoldersSplit = selectedFoldersString.Split(new string[] { "|-F-|" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string folder in selectedFoldersSplit)
				selectedFolders.Add(folder);
			if (selectedFolders.Count == 0)
				selectedFolders.Add(rootFolder);
			selectedFolders.Sort();
		}

		void CheckAndSetWindow(RapidIconWindow w)
		{
			if (!window)
				window = w;
		}

		void CheckAndSetFoldoutColours()
		{
			if (!foldoutColoursSet)
			{
				foldoutStyle = new GUIStyle(EditorStyles.foldout);
				foldoutStyleSelected = new GUIStyle(EditorStyles.foldout);
				Color txtCol = new Color32(192, 192, 192, 255);


				if (EditorGUIUtility.isProSkin)
				{
					//----Set unselected foldout style----
					foldoutStyle.normal.textColor = txtCol;
					foldoutStyle.onNormal.textColor = txtCol;
					foldoutStyle.hover.textColor = txtCol;
					foldoutStyle.onHover.textColor = txtCol;
					foldoutStyle.focused.textColor = txtCol;
					foldoutStyle.onFocused.textColor = txtCol;
					foldoutStyle.active.textColor = txtCol;
					foldoutStyle.onActive.textColor = txtCol;

					//----Set selected foldout style----
					foldoutStyleSelected.normal.textColor = txtCol;
					foldoutStyleSelected.onNormal.textColor = txtCol;
					foldoutStyleSelected.hover.textColor = txtCol;
					foldoutStyleSelected.onHover.textColor = txtCol;
					foldoutStyleSelected.focused.textColor = txtCol;
					foldoutStyleSelected.onFocused.textColor = txtCol;
					foldoutStyleSelected.active.textColor = txtCol;
					foldoutStyleSelected.onActive.textColor = txtCol;
				}
				else
				{
					//----Set unselected foldout style----
					foldoutStyle.normal.textColor = Color.black;
					foldoutStyle.onNormal.textColor = Color.black;
					foldoutStyle.hover.textColor = Color.black;
					foldoutStyle.onHover.textColor = Color.black;
					foldoutStyle.focused.textColor = Color.black;
					foldoutStyle.onFocused.textColor = Color.black;
					foldoutStyle.active.textColor = Color.black;
					foldoutStyle.onActive.textColor = Color.black;

					//----Set selected foldout style----
					foldoutStyleSelected.normal.textColor = Color.white;
					foldoutStyleSelected.onNormal.textColor = Color.white;
					foldoutStyleSelected.hover.textColor = Color.white;
					foldoutStyleSelected.onHover.textColor = Color.white;
					foldoutStyleSelected.focused.textColor = Color.white;
					foldoutStyleSelected.onFocused.textColor = Color.white;
					foldoutStyleSelected.active.textColor = Color.white;
					foldoutStyleSelected.onActive.textColor = Color.white;
				}


				foldoutColoursSet = true;
			}
		}

		void CheckArrowKeys()
		{
			//Check if a key is pressed
			if (assetListFocused && Event.current.isKey && Event.current.type != EventType.KeyUp)
			{
				//----Fold out folders if right arrow key pressed----
				if (Event.current.keyCode == KeyCode.RightArrow)
				{
					foreach (string f in selectedFolders)
					{
						//Fold out subfolders as well if alt pressed
						if (Event.current.alt)
							SetAllFoldoutChildren(f, true);
						else
							foldoutStates[f] = true;
					}
					GUIUtility.ExitGUI();
				}
				//----Collapse folders if left arrow key pressed----
				else if (Event.current.keyCode == KeyCode.LeftArrow)
				{
					foreach (string f in selectedFolders)
					{
						//Fold in subfolders as well if alt pressed
						if (Event.current.alt)
							SetAllFoldoutChildren(f, false);
						else
							foldoutStates[f] = false;
					}
					GUIUtility.ExitGUI();
				}
				//----Select folder below if down arrow key pressed----
				else if (Event.current.keyCode == KeyCode.DownArrow && arrowSelectedFolder < visibleFolders.Count - 1)
				{
					SelectFolder(visibleFolders[++arrowSelectedFolder], -1);
					GUIUtility.ExitGUI();
				}
				//----Select folder above if up arrow key pressed----
				else if (Event.current.keyCode == KeyCode.UpArrow && arrowSelectedFolder > 0)
				{
					SelectFolder(visibleFolders[--arrowSelectedFolder], 1);
					GUIUtility.ExitGUI();
				}
			}
		}

		void CheckFocus(Rect rect)
		{
			//Check if last mouse click was in the asset list rect
			if (Event.current.rawType == EventType.MouseDown)
				assetListFocused = rect.Contains(Event.current.mousePosition);

			if (assetListFocused)
				window.assetGrid.assetGridFocused = false;

			//Check the RapidIcon window is in focus
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() != typeof(RapidIconWindow))
				assetListFocused = false;
		}

		void SetAllFoldoutChildren(string folder, bool foldout)
		{
			//----Set foldout state of folder----
			if (!foldoutStates.ContainsKey(folder))
				foldoutStates.Add(folder, foldout);
			else
				foldoutStates[folder] = foldout;

			//----Set foldout state of sub folders----
			string[] subfolders = AssetDatabase.GetSubFolders(folder);
			foreach (string subfolder in subfolders)
				SetAllFoldoutChildren(subfolder, foldout);
		}

		void SelectFolder(string folder, int keyAdjust)
		{
			assetListFocused = true;
			arrowSelectedFolder = visibleFolders.IndexOf(folder);

			//----Select this folder only----
			if (!Event.current.control && !Event.current.shift)
			{
				selectedFolders.Clear();
				selectedFolders.Add(folder);
				firstSelectedFolder = folder;
				lastSelectedFolder = folder;
			}
			//----Control select, add folder to selection----
			else if (!Event.current.shift)
			{
				if (!selectedFolders.Contains(folder))
				{
					if (selectionMinFolder == "")
						selectionMinFolder = "Assets";

					if (selectionMaxFolder == "")
						selectionMaxFolder = "Assets";

					selectedFolders.Add(folder);
					lastSelectedFolder = folder;

					int lastSelectedFolderIndex = visibleFolders.IndexOf(lastSelectedFolder);

					int selectionMinIndex = visibleFolders.IndexOf(selectionMinFolder);
					if (selectionMinIndex == -1)
						selectionMinIndex = lastSelectedFolderIndex;

					int selectionMaxIndex = visibleFolders.IndexOf(selectionMaxFolder);
					if (selectionMaxIndex == -1)
						selectionMinIndex = lastSelectedFolderIndex;
					
					selectionMinFolder = visibleFolders[Mathf.Min(lastSelectedFolderIndex, selectionMinIndex)];
					selectionMaxFolder = visibleFolders[Mathf.Max(lastSelectedFolderIndex, selectionMaxIndex)];
				}
				else if (selectedFolders.Count > 1)
					selectedFolders.Remove(visibleFolders[visibleFolders.IndexOf(folder) + keyAdjust]);
			}
			//----Shift select, select folder and folders inbetween----
			else
			{
				if (selectionMinFolder == "")
					selectionMinFolder = "Assets";

				if (selectionMaxFolder == "")
					selectionMaxFolder = "Assets";

				if (lastSelectedFolder == "")
					lastSelectedFolder = "Assets";

				if (firstSelectedFolder == "")
					firstSelectedFolder = "Assets";

				int thisFolderIndex = visibleFolders.IndexOf(folder);
				int lastSelectedFolderIndex = visibleFolders.IndexOf(lastSelectedFolder);
				int firstSelectedFolderIndex = visibleFolders.IndexOf(firstSelectedFolder);
				int selectionMinIndex = visibleFolders.IndexOf(selectionMinFolder);
				int selectionMaxIndex = visibleFolders.IndexOf(selectionMaxFolder);
				if (selectionMinIndex == -1)
					selectionMinIndex = int.MaxValue;
				
				int minI = 0, maxI = -1;

				if (thisFolderIndex >= selectionMinIndex && thisFolderIndex <= selectionMaxIndex)
				{
					selectedFolders.Clear();
					minI = Mathf.Min(firstSelectedFolderIndex, thisFolderIndex);
					maxI = Math.Max(firstSelectedFolderIndex, thisFolderIndex);

					selectionMinFolder = visibleFolders[minI];
					selectionMaxFolder = visibleFolders[maxI];
				}
				else
				{
					minI = Mathf.Min(lastSelectedFolderIndex, thisFolderIndex, selectionMinIndex);
					maxI = Mathf.Max(lastSelectedFolderIndex, thisFolderIndex, selectionMaxIndex);

					selectionMinFolder = visibleFolders[Mathf.Min(minI, selectionMinIndex)];
					selectionMaxFolder = visibleFolders[Mathf.Max(maxI, selectionMaxIndex)];
				}

				lastSelectedFolder = visibleFolders[thisFolderIndex];
				for (int i = minI; i <= maxI; i++)
				{
					if (!selectedFolders.Contains(visibleFolders[i]))
						selectedFolders.Add(visibleFolders[i]);
				}
			}

		}

		void UpdateFoldoutStates(string folder, bool foldout)
		{
			if (!foldoutStates.ContainsKey(folder))
				foldoutStates.Add(folder, foldout);
			string[] subfolders = AssetDatabase.GetSubFolders(folder);
			foreach (string subfolder in subfolders)
			{
				UpdateFoldoutStates(subfolder, false);
			}
		}

		void DrawAllSubFolders(string folder, int depth, float width)
		{
			bool inSearch = false;
			foreach (string f in searchFolders)
			{
				if (f.Contains(folder))
				{
					inSearch = true;
					break;
				}
			}
			if (!inSearch && doSearch && depth > 0)
				return;

			string[] subfolders = AssetDatabase.GetSubFolders(folder);

			string[] split = folder.Split('/');
			string displayName = split[split.Length - 1];
			displayName = " " + displayName;
			GUIContent folderGuiContent;

			if (!foldoutStates.ContainsKey(folder))
				UpdateFoldoutStates(folder, false);

			if (EditorGUIUtility.isProSkin || (selectedFolders.Contains(folder) && assetListFocused))
				folderGuiContent = new GUIContent(displayName, foldoutStates[folder] ? folderIcons[1] : folderIcons[0]);
			else
				folderGuiContent = new GUIContent(displayName, foldoutStates[folder] ? folderIcons[3] : folderIcons[2]);


			if (depth == 0)
			{
				foldoutStyle.fontStyle = FontStyle.Bold;
				foldoutStyleSelected.fontStyle = FontStyle.Bold;
			}
			else
			{
				foldoutStyle.fontStyle = FontStyle.Normal;
				foldoutStyleSelected.fontStyle = FontStyle.Normal;
			}

			Rect r = GUILayoutUtility.GetRect(folderGuiContent, GUI.skin.label);
			r.position += new Vector2(15 * depth, 0);
			r.yMin -= 1; r.yMax += 1;

			Rect selectRect = new Rect(r);
			selectRect.position = new Vector2(0, r.position.y);
			selectRect.width = width;
			if (selectedFolders.Contains(folder))
			{
				GUI.DrawTexture(selectRect, assetListFocused ? selectionTextures[0] : selectionTextures[1]);
				if (window)
					window.Repaint();
			}

			if (foldoutStates.ContainsKey(folder))
			{
				visibleFolders.Add(folder);
				if (subfolders.Length > 0)
				{
					Rect clickArea = new Rect(r);
					Rect clickAreaLeft = new Rect(r);
					clickArea.position += new Vector2(15, 0);
					clickArea.width -= 15;
					clickAreaLeft.width = 15 * depth;
					clickAreaLeft.position = new Vector2(0, clickAreaLeft.position.y);
					r.width = 15;
					
					bool chk = foldoutStates[folder];
					if (GUI.Button(clickArea, "", GUIStyle.none) || GUI.Button(clickAreaLeft, "", GUIStyle.none))
					{
						selectFolder = folder;
						window.assetGrid.assetGridFocused = false;
					}

					foldoutStates[folder] = EditorGUI.Foldout(r, foldoutStates[folder], folderGuiContent, (selectedFolders.Contains(folder) && assetListFocused) ? foldoutStyleSelected : foldoutStyle);

					if (foldoutStates[folder] != chk)
					{
						assetListFocused = true;
						GUI.FocusControl(null);

						if (Event.current.alt)
							SetAllFoldoutChildren(folder, foldoutStates[folder]);
					}

					if (foldoutStates[folder])
					{
						foreach (string subfolder in subfolders)
						{
							DrawAllSubFolders(subfolder, depth + 1, width);
						}
						if (depth == 0)
							foldoutStates[folder] = true;
					}
				}
				else
				{
					r.position += new Vector2(15, 0);
					GUIStyle l = new GUIStyle(GUI.skin.label);

					if (EditorGUIUtility.isProSkin)
						l.normal.textColor = new Color32(192, 192, 192, 255);
					else
						l.normal.textColor = (selectedFolders.Contains(folder) && assetListFocused) ? Color.white : Color.black;

					l.hover.textColor = l.normal.textColor;
					l.active.textColor = l.normal.textColor;

					Rect clickAreaLeft = new Rect(r);
					clickAreaLeft.width = 15 * (depth + 1) + 4;
					clickAreaLeft.position = new Vector2(0, clickAreaLeft.position.y);

					if (GUI.Button(r, folderGuiContent, l) || GUI.Button(clickAreaLeft, "", GUIStyle.none))
					{
						selectFolder = folder;
						window.assetGrid.assetGridFocused = false;
					}
				}
			}
		}
	}
}