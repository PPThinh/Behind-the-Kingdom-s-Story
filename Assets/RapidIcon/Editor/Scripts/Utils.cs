using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Animations;
using System.Threading;

namespace RapidIconUIC
{
	static class Utils
	{
		public static Texture2D CreateColourTexture(int width, int height, Color32 c)
		{
			Texture2D tex = new Texture2D(width, height);
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					tex.SetPixel(x, y, c);
				}
			}
			tex.Apply();
			tex.filterMode = FilterMode.Point;

			return tex;
		}

		public static Texture2D RenderIcon(Icon icon, int width = 128, int height = 128)
		{
			GameObject[] sceneGOs = GameObject.FindObjectsOfType<GameObject>();
			for (int i = 0; i < sceneGOs.Length; i++)
			{
				if (sceneGOs[i].activeInHierarchy)
					sceneGOs[i].SetActive(false);
				else
					sceneGOs[i] = null;
			}

			Color ambientLightColour = RenderSettings.ambientLight;
			UnityEngine.Rendering.AmbientMode ambientMode = RenderSettings.ambientMode;
			bool fogEnabled = RenderSettings.fog;

			RenderSettings.ambientLight = icon.ambientLightColour;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
			RenderSettings.fog = false;

			GameObject camGO = new GameObject("cam");
			Camera camC = camGO.AddComponent<Camera>();
			IconRenderer t = camGO.AddComponent<IconRenderer>();
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)icon.assetObject);
			obj.transform.position = icon.objectPosition;
			obj.transform.eulerAngles = icon.objectRotation;
			obj.transform.localScale = icon.objectScale;

			GameObject lightGO = new GameObject("light");
			Light dirLight = lightGO.AddComponent<Light>();
			dirLight.type = LightType.Directional;
			dirLight.color = icon.lightColour;
			dirLight.transform.eulerAngles = icon.lightDir;
			dirLight.intensity = icon.lightIntensity;
			camGO.transform.position = icon.cameraPosition; //new Vector3(0, 2, -2);
															/*if (icon.cameraMode == 0)
																camGO.transform.eulerAngles = icon.cameraRotation;
															else if (icon.cameraMode == 1)*/
			camGO.transform.LookAt(icon.cameraTarget);
			camC.clearFlags = CameraClearFlags.Depth;
			camC.aspect = 1;
			camC.orthographic = icon.cameraOrtho;
			camC.orthographicSize = icon.cameraSize;
			camC.orthographicSize /= icon.camerasScaleFactor;
			camC.fieldOfView = icon.cameraFov;
			camC.nearClipPlane = 0.001f;
			camC.farClipPlane = 10000;
			//camGO.transform.LookAt(obj.transform);
			/*camC.orthographic = true;
			camC.orthographicSize = 1.4f;*/

			if (icon.doAnimation)
			{
				Animator animator = obj.GetComponent<Animator>();
				if (animator != null)
				{
					animator.Play(icon.animationStateHash, icon.animationLayer, icon.animationOffset);
					animator.Update(0);
				}
			}

			t.resolution = new Vector2Int(width, height);
			Texture2D iconRender = t.RenderIcon();

			if (icon.doFixAlphaEdges)
				iconRender = FixAlphaEdges(iconRender);

			Texture2D img = CreateColourTexture(iconRender.width, iconRender.height, Color.clear);

			//img.SetPixels(iconRender.GetPixels());
			//img.Apply();

			//img = ApplyIconShader(img, icon.bgImage, icon.fgImage, icon.mask);
			if (icon.materialToggles == null)
			{
				icon.LoadMatInfo();
				Debug.LogError("[RapidIcon] Undo Error: This is a known bug, if you \"Apply to All Selected Icons\" and then try to undo after changing your icon selection, the tool will not be able to undo the changes.");
			}

			foreach (Material m in icon.postProcessingMaterials)
			{
				if (icon.materialToggles != null)
				{
					if (icon.materialToggles[m])
					{
						var rtd = new RenderTextureDescriptor(img.width, img.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
						var rt = new RenderTexture(rtd);

						if (m == null)
							continue;

						if (m.shader.name == "RapidIcon/ObjectRender")
							m.SetTexture("_Render", iconRender);

						Graphics.Blit(img, rt, m);

						RenderTexture.active = rt;
						img = new Texture2D(img.width, img.height);
						img.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
						img.Apply();
						RenderTexture.active = null;
						rt.Release();
					}
				}
			}

			if (icon.doFixAlphaEdges)
				img = FixAlphaEdges(img);

			UnityEngine.Object.DestroyImmediate(camGO);
			UnityEngine.Object.DestroyImmediate(obj);
			UnityEngine.Object.DestroyImmediate(lightGO);

			for (int i = 0; i < sceneGOs.Length; i++)
			{
				if (sceneGOs[i])
					sceneGOs[i].SetActive(true);
			}

			RenderSettings.ambientLight = ambientLightColour;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.fog = fogEnabled;


			img.filterMode = icon.filterMode;

			return img;
		}

		static Texture2D ApplyIconShader(Texture2D img, Texture2D bg, Texture2D fg, Texture2D mask)
		{
			var rtd = new RenderTextureDescriptor(img.width, img.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);

			if (img == null)
				img = Utils.CreateColourTexture(4, 4, Color.clear);
			if (bg == null)
				bg = Utils.CreateColourTexture(4, 4, Color.clear);
			if (fg == null)
				fg = Utils.CreateColourTexture(4, 4, Color.clear);

			Material mat = new Material(Shader.Find("RapidIcon/ImgLayerShader"));
			mat.SetTexture("_MainTex", img);
			mat.SetTexture("_bgTex", bg);
			mat.SetTexture("_overlayTex", fg);
			mat.SetTexture("_maskTex", mask);
			mat.SetInt("_maskImgLayer", 1);
			mat.SetInt("_maskBGLayer", 1);
			mat.SetInt("_maskFGLayer", 0);
			Graphics.Blit(img, rt, mat);

			RenderTexture.active = rt;
			Texture2D output = new Texture2D(img.width, img.height);
			output.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
			output.Apply();
			RenderTexture.active = null;

			rt.Release();

			output = FixAlphaEdges(output);

			return output;
		}

		static Texture2D FixAlphaEdges(Texture2D tex)
		{
			var rtd = new RenderTextureDescriptor(tex.width, tex.height) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);
			Graphics.Blit(tex, rt, new Material(Shader.Find("RapidIcon/ImgShader")));

			RenderTexture.active = rt;
			Texture2D baked = new Texture2D(tex.width, tex.height);
			baked.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
			baked.Apply();
			RenderTexture.active = null;

			rt.Release();

			return baked;
		}

		public static void SaveIconData(IconData iconData)
		{
			foreach (Icon icon in iconData.icons)
			{
				icon.PrepareForSaveData();
			}

			string data = JsonUtility.ToJson(iconData);
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconData", data);
		}

		public static IconData LoadIconData()
		{
			string data = EditorPrefs.GetString(PlayerSettings.productName + "RapidIconData");
			IconData iconData = JsonUtility.FromJson<IconData>(data);
			if (iconData != null)
			{
				foreach (Icon icon in iconData.icons)
					icon.CompleteLoadData();
			}

			return iconData;
		}

		public static Vector2Int ToVector2Int(this Vector2 v)
		{
			return new Vector2Int((int)v.x, (int)v.y);
		}

		public static void EncapsulateChildBounds(Transform t, ref Bounds bounds)
		{
			MeshRenderer mr;
			for (int i = 0; i < t.childCount; i++)
			{
				mr = t.GetChild(i).GetComponent<MeshRenderer>();
				if (mr != null)
					bounds.Encapsulate(mr.bounds);
				else
				{
					SkinnedMeshRenderer smr = t.GetChild(i).GetComponent<SkinnedMeshRenderer>();
					if (smr != null)
						bounds.Encapsulate(smr.bounds);
				}

				EncapsulateChildBounds(t.GetChild(i), ref bounds);
			}
		}

		[MenuItem("Tools/RapidIcon Utilities/Delete All Saved Data")]
		static void DeleteEditorPrefs()
		{
			if (EditorUtility.DisplayDialog("Confirm", "Are you sure you want to delete all RapidIcon data? This will delete all of your icon settings and cannot be undone", "Delete", "Cancel"))
			{
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconOpenedFolders");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedFolders");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosLeft");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSelectedAssets");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconAssetGridScroll");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosRight");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconSepPosPreview");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconPreviewResIdx");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconPreviewZoomIdx");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconEditorTab");
				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconData");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconIconsRefreshed");

				EditorPrefs.DeleteKey(PlayerSettings.productName + "RapidIconFilterIdx");
			}
		}

		[MenuItem("Tools/RapidIcon Utilities/Don't Save On Close")]
		static void DontSaveOnExit()
		{
			RapidIconWindow.dontSaveOnExit = true;
		}

		public static Vector2Int MutiplyVector2IntByFloat(Vector2Int vec, float f)
		{
			Vector2Int res = vec;
			res.x = (int)(res.x * f);
			res.y = (int)(res.y * f);

			return res;
		}

		public static void Update1_0To1_1(List<Icon> icons)
		{
			Debug.Log("RapidIcon updated to version 1.1, " + icons.Count + " icons updated");
			foreach (Icon icon in icons)
			{

				icon.exportName = icon.assetName;
				int extensionPos = icon.exportName.LastIndexOf('.');
				icon.exportName = icon.exportName.Substring(0, extensionPos);

			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconVersion", "1.1");
		}

		public static void Update1_1To1_2(List<Icon> icons)
		{
			Debug.Log("RapidIcon updated to version 1.2.1, " + icons.Count + " icons updated");
			foreach (Icon icon in icons)
			{
				if (icon.camerasScaleFactor == 0)
					icon.camerasScaleFactor = 1;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconVersion", "1.2.1");
		}

		public static void Update1_2_1To1_3(List<Icon> icons)
		{
			Debug.Log("RapidIcon updated to version 1.3, " + icons.Count + " icons updated");
			foreach (Icon icon in icons)
			{
				icon.doFixAlphaEdges = true;
				icon.filterMode = FilterMode.Point;
			}
			EditorPrefs.SetString(PlayerSettings.productName + "RapidIconVersion", "1.3");
		}
	}
}