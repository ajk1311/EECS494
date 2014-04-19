using UnityEngine;
using UnityEditor;
using System.Collections;

public class Utils : MonoBehaviour {
	[MenuItem("Utils/Create Map Planes From Selection")]
	private static void CreateMapPlanesFromSelection() {
		var mSettings = new MapSettings ("settings.txt");

		var length = mSettings.length;
		var width = mSettings.width;

		var mesh = CreatePlaneMesh(length, width);

		EditorHelper.CreateAssetFolderIfNotExists ("Minimap/Mesh");
		AssetDatabase.CreateAsset (mesh, "Assets/Minimap/Mesh/plane.mesh");

		var selection = Selection.GetFiltered (typeof(Texture), SelectionMode.DeepAssets);
		if(selection != null) {
			for(int i = 0; i < selection.Length; i++) {
				var texture = selection[i] as Texture;
				if(texture != null) {
					var mat = CreateMaterial(texture, texture.name);

					EditorHelper.CreateAssetFolderIfNotExists("Minimap/Prefabs/Materials");
					AssetDatabase.CreateAsset(mat, string.Format("Assets/Minimap/Prefabs/Materials/{0}.mat", texture.name));

					EditorHelper.CreateAssetFolderIfNotExists("Minimap/Prefabs");

					var prefab = PrefabUtility.CreatePrefab(string.Format("Assets/Minimap/Prefabs/{0}.prefab", texture.name), CreatePrefab(mesh, texture.name));

					prefab.renderer.material = mat;
					prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
					prefab.GetComponent<MeshRenderer>().castShadows = false;
					prefab.GetComponent<MeshRenderer>().receiveShadows = false;

//					var prefab = createPrefab(mesh, texture.name);
				}
			}
			AssetDatabase.Refresh();
		}
	}

	private static Material CreateMaterial(Texture texture, string name) {
		var material = new Material (Shader.Find ("Unlit/Texture"));

		material.mainTexture = texture;
		material.name = name;

		return material;
	}

	private static GameObject CreatePrefab(Mesh mesh, string name) {
		var go = new GameObject (name);

		go.AddComponent ("MeshFilter");
		go.AddComponent ("MeshRenderer");
		go.transform.Rotate (new Vector3 (1, 0, 0), 90);

		return go;

	}

	private static Mesh CreatePlaneMesh(int length, int width) {
		var mesh = new Mesh ();
		mesh.name = "low-poly-mesh";

		mesh.vertices = new Vector3[6] {new Vector3(0,0,0), new Vector3(0, width, 0), new Vector3(length, width), 
										new Vector3(length, width), new Vector3(length, 0, 0), new Vector3(0,0,0)};

		mesh.uv = new Vector2[6] { new Vector2(0,0), new Vector2(0,1), new Vector2(1,1), new Vector2(1,1), new Vector2(1,0), new Vector2(0,0)};
		mesh.triangles = new int[6] { 0, 1, 2, 3, 4, 5};

		return mesh;
	}

	[MenuItem("Utils/ExportPackageOptions Seletion To Map Asset")]
	private static void SaveSelectionToMapAsset() {
		var selection = Selection.GetFiltered (typeof(Object), SelectionMode.DeepAssets);
		if(selection != null) {
			var path = EditorUtility.SaveFilePanel("Save Asset", "", "New Data", "dat");
			if(!string.IsNullOrEmpty(path)) {
				var settingsAsset = AssetDatabase.LoadAssetAtPath ("Assets/Minimap/settings.txt", typeof(TextAsset));
				if(settingsAsset == null) {
					Debug.LogError("Settings file not found under minimap folder");
					return;
				}

				BuildPipeline.BuildAssetBundle(settingsAsset, selection, path, BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets);
				Selection.objects = selection;
			}
		}
	}
};
