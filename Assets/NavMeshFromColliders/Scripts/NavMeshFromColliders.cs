#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NavMeshFromColliders
{
    /// <summary>
    /// This class represents a tool that helps to prepare the scene for NavMesh baking.
    /// It will generate clone objects from colliders already in the scene.
    /// These clone objects will contain the renderer generated from the collider shape.
    /// Unity NavMesh baking works upon Renderers, so colliders are ignored.
    /// This tool fulfills the need of using only colliders for NavMesh baking.
    /// </summary>
    public class NavMeshFromCollidersWindow : EditorWindow
    {
        private Stack<Renderer> disableds;
        private LayerMask layerMask = 0;
        private GameObject bakeModeRoot = null;
        private Material diffuseDefaultMaterial = null;
        private bool bakeMode = false;

        [MenuItem("Tools/NavMesh From Colliders")]
        public static void ShowWindow()
        {
            GetWindow<NavMeshFromCollidersWindow>();
        }

        private void Awake()
        {
            layerMask.value = 1;
        }

        private void OnGUI()
        {
            var oldValue = layerMask.value;
            layerMask = CustomEditorExtensions.LayerMaskField("Layer Mask: ", layerMask);
            if (layerMask.value != oldValue)
            {
                Restore();
            }

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

            if (!bakeMode)
            {
                if (GUILayout.Button("Prepare Scene"))
                {
                    bakeMode = true;
                    Prepare();
                }
            }
            else
            {
                if (GUILayout.Button("Restore Scene"))
                {
                    Restore();
                }
            }

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        }

        private void Prepare()
        {
            // Get renderers before generating fakes
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            // Create root for fake objects
            bakeModeRoot = new GameObject("Bake Mode Root");
            bakeModeRoot.transform.parent = null;

            // Generate fake objects based on colliders
            Collider[] colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            for (int i = 0; i < colliders.Length; i++)
            {
                int theTrueMaskSupose = layerMask.value | 1 << colliders[i].gameObject.layer;
                if (layerMask.value == theTrueMaskSupose && !colliders[i].isTrigger)
                {
                    GameObject fakeObject = GenerateRendererObject(colliders[i]);

                    if (fakeObject != null)
                    {
                        fakeObject.transform.parent = bakeModeRoot.transform;
                        fakeObject.layer = colliders[i].gameObject.layer;

                        StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(colliders[i].gameObject);
                        GameObjectUtility.SetStaticEditorFlags(fakeObject, flags);
                    }
                }
            }

            // Disable renderers
            disableds = new Stack<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].enabled)
                {
                    renderers[i].enabled = false;
                    disableds.Push(renderers[i]);
                }
            }

            // You can optionally use the markups later when collecting NavMesh sources, e.g.:
            // NavMeshBuilder.CollectSources(null, layerMask, NavMeshCollectGeometry.RenderMeshes, 0, markups, out var sources);
        }

        private void Restore()
        {
            bakeMode = false;

            // Enable objects again
            while (disableds != null && disableds.Count > 0)
            {
                disableds.Pop().enabled = true;
            }

            // Destroy fake objects
            if (bakeModeRoot != null)
            {
                DestroyImmediate(bakeModeRoot);
                bakeModeRoot = null;
            }
        }

        private GameObject GenerateRendererObject(Collider theCollider)
        {
            GameObject fakeObject = null;
            const string DEFAULT_FAKEOBJECT_NAME = "Object";

            if (theCollider is BoxCollider baseCollider)
            {
                fakeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                DestroyImmediate(fakeObject.GetComponent<Collider>());
                fakeObject.name = DEFAULT_FAKEOBJECT_NAME;
                fakeObject.transform.rotation = theCollider.gameObject.transform.rotation;
                fakeObject.transform.parent = theCollider.gameObject.transform;
                fakeObject.transform.localPosition = baseCollider.center;
                fakeObject.transform.parent = null;
                fakeObject.transform.localScale = theCollider.gameObject.transform.lossyScale;
                Vector3 tempScale = fakeObject.transform.localScale;
                tempScale.Scale(baseCollider.size);
                fakeObject.transform.localScale = tempScale;
            }
            else if (theCollider is CapsuleCollider capsuleCollider)
            {
                fakeObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                DestroyImmediate(fakeObject.GetComponent<Collider>());
                fakeObject.name = DEFAULT_FAKEOBJECT_NAME;
                fakeObject.transform.rotation = theCollider.gameObject.transform.rotation;
                fakeObject.transform.parent = theCollider.gameObject.transform;
                fakeObject.transform.localPosition = capsuleCollider.center;
                fakeObject.transform.parent = null;
                fakeObject.transform.localScale = theCollider.gameObject.transform.lossyScale;

                const float DEFAULT_CAPSULE_RADIUS = 0.5f;
                const float DEFAULT_CAPSULE_HEIGHT = 2.0f;

                Vector3 tempScale = fakeObject.transform.localScale;
                float maxXZ = Mathf.Max(Mathf.Abs(tempScale.x), Mathf.Abs(tempScale.z));
                tempScale.x = tempScale.z = maxXZ;

                tempScale.x *= capsuleCollider.radius / DEFAULT_CAPSULE_RADIUS;
                tempScale.z *= capsuleCollider.radius / DEFAULT_CAPSULE_RADIUS;
                tempScale.y *= capsuleCollider.height / DEFAULT_CAPSULE_HEIGHT;
                fakeObject.transform.localScale = tempScale;
            }
            else if (theCollider is SphereCollider sphereCollider)
            {
                fakeObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                DestroyImmediate(fakeObject.GetComponent<Collider>());
                fakeObject.name = DEFAULT_FAKEOBJECT_NAME;
                fakeObject.transform.rotation = theCollider.gameObject.transform.rotation;
                fakeObject.transform.parent = theCollider.gameObject.transform;
                fakeObject.transform.localPosition = sphereCollider.center;
                fakeObject.transform.parent = null;
                fakeObject.transform.localScale = theCollider.gameObject.transform.lossyScale;

                const float DEFAULT_SPHERE_RADIUS = 0.5f;
                Vector3 tempScale = fakeObject.transform.localScale;
                float maxScale = Mathf.Max(Mathf.Abs(tempScale.x), Mathf.Abs(tempScale.y), Mathf.Abs(tempScale.z));
                tempScale = Vector3.one * (maxScale * (sphereCollider.radius / DEFAULT_SPHERE_RADIUS));
                fakeObject.transform.localScale = tempScale;
            }
            else if (theCollider is MeshCollider meshCollider)
            {
                if (meshCollider.GetComponent<MeshRenderer>() != null)
                {
                    int materialsCount = meshCollider.GetComponent<MeshRenderer>().sharedMaterials.Length;
                    fakeObject = new GameObject(DEFAULT_FAKEOBJECT_NAME);
                    fakeObject.transform.SetPositionAndRotation(theCollider.gameObject.transform.position,
                        theCollider.gameObject.transform.rotation);

                    fakeObject.AddComponent<MeshFilter>().sharedMesh = meshCollider.sharedMesh;

                    if (diffuseDefaultMaterial == null)
                    {
                        SetDefaultMaterialReference();
                    }

                    Material[] mats = new Material[materialsCount];
                    for (int i = 0; i < mats.Length; i++)
                    {
                        mats[i] = diffuseDefaultMaterial;
                    }

                    fakeObject.AddComponent<MeshRenderer>().materials = mats;
                    fakeObject.transform.localScale = theCollider.gameObject.transform.lossyScale;
                }
            }

            return fakeObject;
        }

        private void SetDefaultMaterialReference()
        {
            GameObject tempPrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            diffuseDefaultMaterial = tempPrimitive.GetComponent<MeshRenderer>().sharedMaterial;
            DestroyImmediate(tempPrimitive);
        }

        private void OnDestroy()
        {
            Restore();
        }
    }

    /// <summary>
    /// Custom editor extensions used for LayerMaskField, etc.
    /// </summary>
    public static class CustomEditorExtensions
    {
        public static LayerMask LayerMaskField(string label, LayerMask layerMask)
        {
            List<string> layers = new();
            List<int> layerNumbers = new();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0) maskWithoutEmpty |= (1 << i);
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0) mask |= (1 << layerNumbers[i]);
            }

            layerMask.value = mask;
            return layerMask;
        }
    }
}

#endif