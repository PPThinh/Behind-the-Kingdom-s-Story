using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Threading;
using System.Diagnostics.SymbolStore;

namespace RapidIconUIC
{
	public class RapidIconWindow : EditorWindow
	{
		public static RapidIconWindow window;

		public AssetList assetList;
		public DraggableSeparator leftSeparator, rightSeparator;
		public AssetGrid assetGrid;
		public IconEditor iconEditor;
		public static bool dontSaveOnExit;

		[MenuItem("Tools/RapidIcon")]
		public static void Init()
		{
			/*--------------------------------------------------------------------------------
			 * Display RapidIcon window
			 *--------------------------------------------------------------------------------*/
			dontSaveOnExit = false;
			window = (RapidIconWindow)GetWindow(typeof(RapidIconWindow), false, "RapidIcon");
			window.minSize = new Vector2(870, 600);
			window.Show();

			float x = EditorPrefs.GetFloat(PlayerSettings.productName + "RapidIconWindowPosX", -1);
			float y = EditorPrefs.GetFloat(PlayerSettings.productName + "RapidIconWindowPosY", -1);
			float width = EditorPrefs.GetFloat(PlayerSettings.productName + "RapidIconWindowWidth", -1);

			if (x != -1 && y!= -1 && width != -1)
				window.position = new Rect(x, y, width, window.position.height);

			EditorSceneManager.sceneClosing += window.SceneClosing;
			EditorSceneManager.sceneOpened += window.OpenScene;
			EditorSceneManager.newSceneCreated += window.NewScene;
		}

		private void OnEnable()
		{
			/*--------------------------------------------------------------------------------
			 * Instantiate and initialise window elements
			 * Load data
			 *--------------------------------------------------------------------------------*/
			assetList = new AssetList(Application.dataPath);
			assetList.LoadData();

			leftSeparator = new DraggableSeparator(SeparatorTypes.Vertical);
			leftSeparator.LoadData("RapidIconSepPosLeft", 300);

			rightSeparator = new DraggableSeparator(SeparatorTypes.Vertical);
			rightSeparator.LoadData("RapidIconSepPosRight", 900);

			assetGrid = new AssetGrid(assetList);

			iconEditor = new IconEditor(assetGrid, window);
			iconEditor.LoadData();

			assetGrid.LoadData();
		}

		void SceneClosing(Scene s, bool removingScene)
		{
			foreach(Icon icon in assetGrid.objectIcons.Values)
				icon.SaveMatInfo();	
		}

		void NewScene(Scene s, NewSceneSetup newSceneSetup, NewSceneMode newSceneMode)
		{
			window.Close();
		}

		void OpenScene(Scene s, OpenSceneMode openSceneMode)
		{
			window.Close();
		}

		private void OnDisable()
		{
			/*--------------------------------------------------------------------------------
			 * Save data
			 *--------------------------------------------------------------------------------*/
			assetList.SaveData();
			leftSeparator.SaveData("RapidIconSepPosLeft");
			rightSeparator.SaveData("RapidIconSepPosRight");
			assetGrid.SaveData();

			if (dontSaveOnExit)
			{
				bool res = EditorUtility.DisplayDialog("Exit Without Save?", "You have selected to exit without saving, are you sure?", "Don't Save", "Save");
				if (res)
					return;
			}

			iconEditor.SaveData();
		}

		void OnGUI()
		{
			if (!window)
				Init();

			/*--------------------------------------------------------------------------------
			 * Draw window elements
			 *--------------------------------------------------------------------------------*/
			GUILayout.BeginHorizontal();
			if (!iconEditor.fullscreen)
			{
				assetList.Draw(leftSeparator.value, window);
				leftSeparator.Draw(150, rightSeparator.value - 320, window);
				assetGrid.Draw(rightSeparator.value - leftSeparator.value, window);
				rightSeparator.Draw(leftSeparator.value + 320, window.position.width - 400, window);
			}
			iconEditor.Draw(window.position.width - rightSeparator.value, window);
			GUILayout.EndHorizontal();
		}
	}
}