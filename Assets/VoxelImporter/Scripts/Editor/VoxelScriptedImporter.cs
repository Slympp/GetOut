#if UNITY_2017_1_OR_NEWER

using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace VoxelImporter
{
    [ScriptedImporter(EditorDataVersion, new string[] { "vox", "qb" })]
    public class VoxelScriptedImporter : ScriptedImporter
    {
        public const int EditorDataVersion = 4;
        public int dataVersion;
        public VoxelBase.FileType fileType;
        public bool legacyVoxImport;
        public VoxelBase.ImportMode importMode = VoxelBase.ImportMode.LowPoly;
        public Vector3 importScale = Vector3.one;
        public Vector3 importOffset;
        public bool combineFaces = true;
        public bool ignoreCavity = true;
        public bool shareSameFace = true;
        public bool outputStructure;
        public bool generateLightmapUVs;
        public float generateLightmapUVsAngleError = 8f;
        public float generateLightmapUVsAreaError = 15f;
        public float generateLightmapUVsHardAngle = 88f;
        public float generateLightmapUVsPackMargin = 4f;
        public bool generateTangents;
        public float meshFaceVertexOffset;
        public bool loadFromVoxelFile = true;
        public bool generateMipMaps;
        public enum ColliderType
        {
            None,
            Box,
            Sphere,
            Capsule,
            Mesh,
        }
        public ColliderType colliderType;
        public Material[] materials;
        public string[] materialNames;
        [Serializable]
        public class MaterialRemap
        {
            public string name;
            public Material material;
        }
        public MaterialRemap[] remappedMaterials;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            {
                var ext = Path.GetExtension(ctx.assetPath).ToLower();
                if (ext == ".vox") fileType = VoxelBase.FileType.vox;
                else if (ext == ".qb") fileType = VoxelBase.FileType.qb;
                else return;
            }

            #region DefaultScale
            if (dataVersion == 0 &&
                importScale == Vector3.one &&
                AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ctx.assetPath) == null)
            {
                var x = EditorPrefs.GetFloat("VoxelImporter_DefaultScaleX", 1f);
                var y = EditorPrefs.GetFloat("VoxelImporter_DefaultScaleY", 1f);
                var z = EditorPrefs.GetFloat("VoxelImporter_DefaultScaleZ", 1f);
                importScale = new Vector3(x, y, z);
            }
            #endregion

            Action<string> LogImportError = (log) =>
            {
#if UNITY_2018_1_OR_NEWER
                ctx.LogImportError(log);
#else
                Debug.LogError(log);
#endif
            };

            var gameObject = new GameObject(Path.GetFileNameWithoutExtension(ctx.assetPath));
            var voxelObject = gameObject.AddComponent<VoxelObject>();
            {
                voxelObject.legacyVoxImport = legacyVoxImport;
                voxelObject.importMode = importMode;
                voxelObject.importScale = importScale;
                voxelObject.importOffset = importOffset;
                voxelObject.combineFaces = combineFaces;
                voxelObject.ignoreCavity = ignoreCavity;
                voxelObject.shareSameFace = shareSameFace;
                voxelObject.voxelStructure = outputStructure ? ScriptableObject.CreateInstance<VoxelStructure>() : null;
                voxelObject.generateLightmapUVs = generateLightmapUVs;
                voxelObject.generateLightmapUVsAngleError = generateLightmapUVsAngleError;
                voxelObject.generateLightmapUVsAreaError = generateLightmapUVsAreaError;
                voxelObject.generateLightmapUVsHardAngle = generateLightmapUVsHardAngle;
                voxelObject.generateLightmapUVsPackMargin = generateLightmapUVsPackMargin;
                voxelObject.generateTangents = generateTangents;
                voxelObject.meshFaceVertexOffset = meshFaceVertexOffset;
                voxelObject.loadFromVoxelFile = loadFromVoxelFile;
                voxelObject.generateMipMaps = generateMipMaps;
            }
            var objectCore = new VoxelObjectCore(voxelObject);
            try
            {
                if (!objectCore.Create(ctx.assetPath, null))
                {
                    LogImportError(string.Format("<color=green>[Voxel Importer]</color> ScriptedImporter error. file:{0}", ctx.assetPath));
                    DestroyImmediate(gameObject);
                    return;
                }
            }
            catch
            {
                LogImportError(string.Format("<color=green>[Voxel Importer]</color> ScriptedImporter error. file:{0}", ctx.assetPath));
                DestroyImmediate(gameObject);
                return;
            }

            #region Correspondence in Issue ID 947055 Correction in case before correction is applied
            for (int i = 0; i < voxelObject.materials.Count; i++)
            {
                if (voxelObject.materials[i] != null)
                    voxelObject.materials[i].hideFlags |= HideFlags.NotEditable;
            }
            if (voxelObject.atlasTexture != null)
                voxelObject.atlasTexture.hideFlags |= HideFlags.NotEditable;
            if (voxelObject.mesh != null)
                voxelObject.mesh.hideFlags |= HideFlags.NotEditable;
            #endregion

            #region Material
            {
                materials = new Material[voxelObject.materialIndexes.Count];
                materialNames = new string[voxelObject.materialIndexes.Count];
                for (int i = 0; i < voxelObject.materialIndexes.Count; i++)
                {
                    var index = voxelObject.materialIndexes[i];
                    var material = voxelObject.materials[index];
                    material.name = string.Format("mat{0}", index);
                    materials[i] = material;
                    materialNames[i] = material.name;
                }
                if (remappedMaterials != null)
                {
                    remappedMaterials = remappedMaterials.Where(item => item.material != null).ToArray();
                }
            }
            #endregion

            #region Collider
            switch (colliderType)
            {
            case ColliderType.Box:
                gameObject.AddComponent<BoxCollider>();
                break;
            case ColliderType.Sphere:
                gameObject.AddComponent<SphereCollider>();
                break;
            case ColliderType.Capsule:
                gameObject.AddComponent<CapsuleCollider>();
                break;
            case ColliderType.Mesh:
                gameObject.AddComponent<MeshCollider>();
                break;
            }
            #endregion

#if UNITY_2017_3_OR_NEWER
            ctx.AddObjectToAsset(gameObject.name, gameObject);
            ctx.AddObjectToAsset(voxelObject.mesh.name = "mesh", voxelObject.mesh);
            {
                var materials = new List<Material>();
                for (int i = 0; i < voxelObject.materialIndexes.Count; i++)
                {
                    var material = voxelObject.materials[voxelObject.materialIndexes[i]];
                    materials.Add(material);
                    if (remappedMaterials != null)
                    {
                        var index = ArrayUtility.FindIndex(remappedMaterials, (t) => { return t.name == material.name; });
                        if (index >= 0)
                        {
                            materials[i] = remappedMaterials[index].material;
                            continue;
                        }
                    }
                    ctx.AddObjectToAsset(material.name, material);
                }
                gameObject.GetComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
            }
            ctx.AddObjectToAsset(voxelObject.atlasTexture.name = "tex", voxelObject.atlasTexture);
            if (voxelObject.voxelStructure != null)
            {
                ctx.AddObjectToAsset(voxelObject.voxelStructure.name = "structure", voxelObject.voxelStructure);
            }

            VoxelObject.DestroyImmediate(voxelObject);

            ctx.SetMainObject(gameObject);
#else
            ctx.SetMainAsset(gameObject.name, gameObject);
            ctx.AddSubAsset(voxelObject.mesh.name = "mesh", voxelObject.mesh);
            for (int i = 0; i < voxelObject.materialIndexes.Count; i++)
            {
                var material = voxelObject.materials[voxelObject.materialIndexes[i]];
                ctx.AddSubAsset(material.name, material);
            }
            ctx.AddSubAsset(voxelObject.atlasTexture.name = "tex", voxelObject.atlasTexture);
            if (voxelObject.voxelStructure != null)
            {
                ctx.AddSubAsset(voxelObject.voxelStructure.name = "structure", voxelObject.voxelStructure);
            }

            VoxelObject.DestroyImmediate(voxelObject);
#endif
            dataVersion = EditorDataVersion;
        }
    }
}
#endif