using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Object = UnityEngine.Object;

namespace NGS.AdvancedCullingSystem.Dynamic
{
    public static class DC_ControllerEditorUtil
    {
        public static bool ContainsReadyToBakeSources(DC_SourceSettings[] sources)
        {
            foreach (var settings in sources)
            {
                if (!settings.IsIncompatible && !settings.Baked)
                    return true;
            }

            return false;
        }

        public static bool ContainsBakedSources(DC_SourceSettings[] sources)
        {
            foreach (var settings in sources)
            {
                if (settings.Baked)
                    return true;
            }

            return false;
        }

        public static bool ContainsNonReadableMeshes(DC_SourceSettings[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                DC_SourceSettings source = sources[i];

                if (source.SourceType == SourceType.SingleMesh)
                {
                    if (source.TryGetComponent(out MeshFilter filter))
                    {
                        Mesh mesh = filter.sharedMesh;

                        if (mesh != null && !mesh.isReadable)
                            return true;
                    }
                }
                else
                {
                    foreach (MeshFilter filter in source.GetComponentsInChildren<MeshFilter>())
                    {
                        Mesh mesh = filter.sharedMesh;

                        if (mesh != null && !mesh.isReadable)
                            return true;
                    }
                }
            }

            return false;
        }


        public static void MakeMeshesReadable()
        {
            DC_SourceSettings[] sources = Object.FindObjectsOfType<DC_SourceSettings>();
            int count = 0;

            for (int i = 0; i < sources.Length; i++)
            {
                DC_SourceSettings source = sources[i];

                if (source.SourceType == SourceType.SingleMesh)
                {
                    if (source.TryGetComponent(out MeshFilter filter))
                    {
                        if (filter.sharedMesh != null)
                            if (MakeMeshReadable(filter.sharedMesh))
                                count++;
                    }
                }
                else
                {
                    foreach (MeshFilter filter in source.GetComponentsInChildren<MeshFilter>())
                    {
                        if (filter.sharedMesh != null)
                            if (MakeMeshReadable(filter.sharedMesh))
                                count++;
                    }
                }
            }

            Debug.Log(count + " meshes maked readable");
        }

        public static void BakeScene()
        {
            try
            {
                DC_SourceSettings[] sources = Object.FindObjectsOfType<DC_SourceSettings>();
                int count = sources.Length;

                for (int i = 0; i < count; i++)
                {
                    EditorUtility.DisplayProgressBar("Bake...", i + " of " + count, (float)i / count);
                    sources[i].Bake();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        public static void ClearBakedData()
        {
            try
            {
                DC_SourceSettings[] sources = Object.FindObjectsOfType<DC_SourceSettings>();
                int count = sources.Length;

                for (int i = 0; i < count; i++)
                {
                    EditorUtility.DisplayProgressBar("Clear...", i + " of " + count, (float)i / count);
                    sources[i].ClearBakedData();
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }


        private static bool MakeMeshReadable(Mesh mesh)
        {
            try
            {
                if (mesh.isReadable)
                    return false;

                string path = AssetDatabase.GetAssetPath(mesh.GetInstanceID());

                if (path == null || path == "")
                    Debug.Log("Unable find path for mesh : " + mesh.name);

                ModelImporter importer = (ModelImporter)AssetImporter.GetAtPath(path);

                importer.isReadable = true;
                importer.SaveAndReimport();

                Debug.Log(string.Format("Maked {0} mesh readable", mesh.name));

                return true;
            }
            catch (Exception ex)
            {
                Debug.Log(string.Format("Unable to make mesh {0} readable. Reason : {1}{2}",
                    mesh.name, ex.Message, ex.StackTrace));

                return false;
            }
        }
    }
}