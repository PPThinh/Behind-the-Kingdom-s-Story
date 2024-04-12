using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using UnityEngine.Rendering;
using UnityEditor.UIElements;
using System.Xml;

namespace RapidIconUIC
{
	[Serializable]
	public class Icon
	{
		[Serializable]
		public struct MatProperty<T>
		{
			public string name;
			public T value;

			public MatProperty(string n, T v)
			{
				name = n;
				value = v;
			}
		}

		[Serializable]
		public struct MaterialInfo
		{
			public string shaderName;
			public string displayName;
			public bool toggle;
			public List<MatProperty<int>> intProperties;
			public List<MatProperty<float>> floatProperties;
			public List<MatProperty<float>> rangeProperties;
			public List<MatProperty<Color>> colourProperties;
			public List<MatProperty<Vector4>> vectorProperties;
			public List<MatProperty<string>> textureProperties;

			public MaterialInfo(string n)
			{
				shaderName = n;
				displayName = n;
				toggle = true;
				intProperties = new List<MatProperty<int>>();
				floatProperties = new List<MatProperty<float>>();
				rangeProperties = new List<MatProperty<float>>();
				colourProperties = new List<MatProperty<Color>>();
				vectorProperties = new List<MatProperty<Vector4>>();
				textureProperties = new List<MatProperty<string>>();
			}
		}

		public string assetPath, folderPath, assetName, assetShortenedName, exportFolderPath, exportName, exportPrefix, exportSuffix;
		public Texture2D previewRender, fullRender, selectionTexture;//bgImage, fgImage, mask
		public UnityEngine.Object assetObject;
		public bool selected, saveData, deleted, cameraOrtho;
		public string[] GUIDs;
		public Vector3 cameraPosition, cameraRotation, objectPosition, objectRotation, objectScale;
		public Vector2Int exportResolution;
		public int assetGridIconIndex;
		public float cameraFov, cameraSize, camerasScaleFactor;
		public List<Material> postProcessingMaterials;
		public Color lightColour;
		public Vector3 lightDir;
		public float lightIntensity;
		public Color ambientLightColour;
		//public int cameraMode; //0: pos/rot, 1: target
		public Vector3 cameraTarget;

		public Dictionary<Material, String> materialDisplayNames;
		public Dictionary<Material, bool> materialToggles;

		public List<MaterialInfo> matInfo;
		public bool doFixAlphaEdges;
		public FilterMode filterMode;

		public Animator animator;
		public AnimatorController animatorController;
		public string[] animLayerNames;
		public string[][] animStateNames;
		public int[][] animStateHashes;
		public int animationLayer;
		public int animationStateIdx;
		public int animationStateHash;
		public float animationOffset;
		public bool doAnimation;
		[SerializeField] string dictSerialised;

		public Icon(Shader objRenderShader, string rapidIconRootFolder, string path)
		{
			cameraPosition = new Vector3(100, Mathf.Sqrt(2 * 100 * 100), 100);
			cameraRotation = new Vector3(45, 225, 0);
			cameraOrtho = true;
			cameraFov = 1;
			cameraSize = 5;
			camerasScaleFactor = 1;
			objectPosition = Vector3.zero;
			objectRotation = Vector3.zero;
			objectScale = Vector3.one;
			ambientLightColour = Color.gray;
			lightColour = Color.white;
			lightDir = new Vector3(50, -30, 0);
			lightIntensity = 1;
			exportResolution = new Vector2Int(256, 256);
			postProcessingMaterials = new List<Material>();
			matInfo = new List<MaterialInfo>();
			materialDisplayNames = new Dictionary<Material, string>();
			materialToggles = new Dictionary<Material, bool>();
			//cameraMode = 1;
			cameraTarget = Vector3.zero;

			filterMode = FilterMode.Point;

			doFixAlphaEdges = true;

			Material defaultRender = new Material(objRenderShader);
			postProcessingMaterials.Add(defaultRender);
			materialDisplayNames.Add(defaultRender, "Object Render");
			materialToggles.Add(defaultRender, true);

			exportFolderPath = rapidIconRootFolder;
			exportFolderPath += "Exports/";

		}

		public void LoadAnimations()
		{
			GameObject gameObject = null;
			try
			{
				gameObject = PrefabUtility.LoadPrefabContents(assetPath);
				animator = gameObject.GetComponent<Animator>();
			}
			catch
			{
				//Not a prefab - don't need to do anything just catch the error and exit
				UnloadPrefabContents(gameObject);
				return;
			}

			if (animator == null)
			{
				UnloadPrefabContents(gameObject);
				return;
			}

			animatorController = animator.runtimeAnimatorController as AnimatorController;

			if (animatorController == null)
			{
				UnloadPrefabContents(gameObject);
				return;
			}

			var layers = animatorController.layers;
			animLayerNames = new string[layers.Length];
			animStateNames = new string[layers.Length][];
			animStateHashes = new int[layers.Length][];
			for(int i = 0; i < layers.Length; i++)
			{
				animLayerNames[i] = layers[i].name;

				var states = layers[i].stateMachine.states;
				animStateNames[i] = new string[states.Length];
				animStateHashes[i] = new int[states.Length];
				for(int j=0;j<states.Length;j++)
				{
					animStateNames[i][j] = states[j].state.name;
					animStateHashes[i][j] = states[j].state.nameHash;
				}
			}

			UnloadPrefabContents(gameObject);
		}

		private void UnloadPrefabContents(GameObject g)
		{
			if (g != null)
				PrefabUtility.UnloadPrefabContents(g);
		}

		public void Update(Vector2Int fullRenderSize)
		{
			fullRender = Utils.RenderIcon(this, fullRenderSize.x, fullRenderSize.y);
		}

		public void Update(Vector2Int fullRenderSize, Vector2Int previewSize)
		{
			previewRender = Utils.RenderIcon(this, previewSize.x, previewSize.y);
			fullRender = Utils.RenderIcon(this, fullRenderSize.x, fullRenderSize.y);
		}

		public void CompleteLoadData()
		{
			if (GUIDs != null)
			{
				//bgImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDs[0]));
				//fgImage = (Texture2D)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDs[1]));
				//mask = (Texture2D)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDs[2]));
				assetObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(GUIDs[3]));
			}
			GUIDs = new string[0];
		
			postProcessingMaterials = new List<Material>();
			materialDisplayNames = new Dictionary<Material, string>();
			materialToggles = new Dictionary<Material, bool>();

			LoadMatInfo();

			DeserialiseDict();
		}

		public void PrepareForSaveData()
		{
			GUIDs = new string[4];
			//GUIDs[0] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bgImage));
			//GUIDs[1] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(fgImage));
			//GUIDs[2] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mask));
			GUIDs[3] = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assetObject));
			//bgImage = fgImage = mask = null;
			previewRender = fullRender = null;
			assetObject = null;

			SaveMatInfo();

			postProcessingMaterials.Clear();
			materialDisplayNames.Clear();
			materialToggles.Clear();

			SerialiseDict();
		}

		private void SerialiseDict()
		{
			if (animStateNames == null || animStateHashes == null)
				return;

			dictSerialised = animStateNames.Length + "\n";

			for (int i = 0; i < animStateNames.Length; i++)
			{
				for (int j = 0; j < animStateNames[i].Length; j++)
				{
					dictSerialised += animStateNames[i][j] + ":" + animStateHashes[i][j] + "|";
				}
				dictSerialised += "\n";
			}
		}

		private void DeserialiseDict()
		{
			if (dictSerialised != "")
			{
				string[] lines = dictSerialised.Split('\n');
				int length = 0;
				bool success = int.TryParse(lines[0], out length);
				if (!success)
					return;

				animStateNames = new string[length][];
				animStateHashes = new int[length][];

				for (int i = 0; i < length; i++)
				{
					string[] entries = lines[i+1].Split('|');
					animStateNames[i] = new string[entries.Length-1];
					animStateHashes[i] = new int[entries.Length-1];

					for (int j = 0; j < entries.Length-1; j++)
					{
						string[] split = entries[j].Split(':');
						animStateNames[i][j] = split[0];
						animStateHashes[i][j] = int.Parse(split[1]);
					}
				}
			}
		}

		public void LoadMatInfo()
		{
			if (matInfo != null && matInfo.Count > 0 && saveData)
			{
				//Debug.Log("[" + assetName + "] Loading mat info, " + matInfo.Count + " materials");
				postProcessingMaterials = new List<Material>();
				materialDisplayNames = new Dictionary<Material, string>();
				materialToggles = new Dictionary<Material, bool>();

				foreach (MaterialInfo mi in matInfo)
				{
					Material m = new Material(Shader.Find(mi.shaderName));

					foreach (MatProperty<int> property in mi.intProperties)
					{
						if (property.name == "PreMulAlpha")
							doFixAlphaEdges = (property.value == 1 ? true : false);
#if UNITY_2021_1_OR_NEWER //Not implemented in older versions of Unity
						m.SetInt(property.name, property.value);
#endif
					}


					foreach (MatProperty<float> property in mi.floatProperties)
						m.SetFloat(property.name, property.value);

					foreach (MatProperty<float> property in mi.rangeProperties)
						m.SetFloat(property.name, property.value);

					foreach (MatProperty<Color> property in mi.colourProperties)
						m.SetColor(property.name, property.value);

					foreach (MatProperty<Vector4> property in mi.vectorProperties)
						m.SetVector(property.name, property.value);

					foreach (MatProperty<string> property in mi.textureProperties)
					{
						if (property.name != "null")
						{
							m.SetTexture(property.name, (Texture2D)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(property.value)));
						}
					}
					postProcessingMaterials.Add(m);
					materialDisplayNames.Add(m, mi.displayName);
					materialToggles.Add(m, mi.toggle);
				}
			}
		}

		public void SaveMatInfo()
		{
			//Debug.Log("[" + assetName + "] Saving mat info, " + postProcessingMaterials.Count + " materials (previous: " + matInfo.Count + ")");

			matInfo.Clear();
			foreach (Material mat in postProcessingMaterials)
			{
				if (mat == null)
					continue;

				MaterialInfo mi = new MaterialInfo(mat.shader.name);

				int propCount = mat.shader.GetPropertyCount();
				for (int i = 0; i < propCount; i++)
				{
					string propName = mat.shader.GetPropertyName(i);
					UnityEngine.Rendering.ShaderPropertyType propType = mat.shader.GetPropertyType(i);

					switch (propType)
					{
#if UNITY_2021_1_OR_NEWER //Not implemented in older versions of Unity
						case UnityEngine.Rendering.ShaderPropertyType.Int:
							mi.intProperties.Add(new MatProperty<int>(propName, mat.GetInt(propName)));
							break;
#endif

						case UnityEngine.Rendering.ShaderPropertyType.Float:
							mi.floatProperties.Add(new MatProperty<float>(propName, mat.GetFloat(propName)));
							break;

						case UnityEngine.Rendering.ShaderPropertyType.Range:
							mi.rangeProperties.Add(new MatProperty<float>(propName, mat.GetFloat(propName)));
							break;

						case UnityEngine.Rendering.ShaderPropertyType.Color:
							mi.colourProperties.Add(new MatProperty<Color>(propName, mat.GetColor(propName)));
							break;

						case UnityEngine.Rendering.ShaderPropertyType.Vector:
							mi.vectorProperties.Add(new MatProperty<Vector4>(propName, mat.GetVector(propName)));
							break;

						case UnityEngine.Rendering.ShaderPropertyType.Texture:
							Texture t = mat.GetTexture(propName);
							if (t != null)
								mi.textureProperties.Add(new MatProperty<string>(propName, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t))));
							else
								mi.textureProperties.Add(new MatProperty<string>(propName, "null"));
							break;
					}

				}

				if(mat.shader.name == "RapidIcon/ObjectRender")
				{
					mi.intProperties.Add(new MatProperty<int>("PreMulAlpha", doFixAlphaEdges ? 1 : 0));
				}

				mi.displayName = materialDisplayNames[mat];
				mi.toggle = materialToggles[mat];
				matInfo.Add(mi);
			}
		}
	}
}