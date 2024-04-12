using UnityEngine;

namespace RapidIconUIC
{

	public class IconRenderer : MonoBehaviour
	{
		public Vector2Int resolution;
		public Texture2D icon;

#if UNITY_EDITOR
		public Texture2D RenderIcon()
		{
			resolution.x = (int)Mathf.Clamp(resolution.x, 1, 2048);
			resolution.y = (int)Mathf.Clamp(resolution.y, 1, 2048);

			var cam = GetComponent<Camera>();

			var rtd = new RenderTextureDescriptor(resolution.x, resolution.y) { depthBufferBits = 24, msaaSamples = 8, useMipMap = false, sRGB = true };
			var rt = new RenderTexture(rtd);
			rt.Create();

			cam.targetTexture = rt;
			cam.aspect = (float)resolution.x / (float)resolution.y;
			cam.Render();
			cam.targetTexture = null;
			cam.ResetAspect();


			icon = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, true, false);


			var oldActive = RenderTexture.active;
			RenderTexture.active = rt;
			icon.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
			icon.Apply();
			RenderTexture.active = oldActive;
			rt.Release();

			return icon;

		}

#endif
	}
}