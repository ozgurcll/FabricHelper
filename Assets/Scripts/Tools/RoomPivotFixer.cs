using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RoomPivotFixer : EditorWindow
{
    [MenuItem("Tools/Oda Pivot Düzeltici")]
    public static void ShowWindow()
    {
        GetWindow<RoomPivotFixer>("Pivot Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Seçili Odaların Pivotunu Merkeze Taşı", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Kullanım: Hierarchy'den Oda Parentlarını seç ve butona bas.");

        if (GUILayout.Button("Seçili Objelerin Pivotunu Düzelt"))
        {
            FixPivots();
        }
    }

    private void FixPivots()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("Lütfen Hiyerarşiden Oda Parentlarını seçin!");
            return;
        }

        foreach (GameObject parentObj in selectedObjects)
        {
            Undo.RecordObject(parentObj.transform, "Fix Pivot");

            // 1. Çocukların merkezini bul
            Vector3 center = GetCenterPoint(parentObj.transform);

            // Eğer çocuk yoksa veya renderer yoksa atla
            if (center == Vector3.zero && parentObj.transform.childCount == 0) continue;

            // 2. Parent hareket edince çocuklar da kaymasın diye çocukları geçici olarak dışarı al
            List<Transform> children = new List<Transform>();
            // Tersten döngü, çünkü child sayısını değiştiriyoruz
            for (int i = parentObj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = parentObj.transform.GetChild(i);
                Undo.RecordObject(child, "Unparent Child");
                children.Add(child);
                child.SetParent(null); // Sahneye bırak
            }

            // 3. Parent'ı merkeze taşı
            parentObj.transform.position = center;

            // 4. Çocukları tekrar içine at
            foreach (Transform child in children)
            {
                Undo.RecordObject(child, "Reparent Child");
                child.SetParent(parentObj.transform);
            }
            
            Debug.Log($"{parentObj.name} pivotu {center} noktasına taşındı.");
        }
    }

    private Vector3 GetCenterPoint(Transform parent)
    {
        Bounds bounds = new Bounds(parent.position, Vector3.zero);
        bool hasBounds = false;

        // Tüm rendererları (meshleri) bul
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            foreach (Renderer r in renderers)
            {
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }
        }
        else
        {
            // Renderer yoksa transform pozisyonlarına bak
            Transform[] allTransforms = parent.GetComponentsInChildren<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t == parent) continue;
                if (!hasBounds)
                {
                    bounds = new Bounds(t.position, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(t.position);
                }
            }
        }

        return bounds.center;
    }
}