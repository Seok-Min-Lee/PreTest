using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GizmoRotateHandleSetup
{
    private const string RingMeshPath = "Assets/Models/ring.fbx";

    [MenuItem("Tools/Gizmo/Rotate 핸들에 Ring 메시 적용")]
    private static void ApplyRingMeshToRotateHandles()
    {
        Mesh ringMesh = AssetDatabase.LoadAssetAtPath<Mesh>(RingMeshPath);

        if (ringMesh == null)
        {
            Debug.LogError($"{RingMeshPath}에서 Mesh를 찾지 못했습니다.");
            return;
        }

        GizmoHandle[] handles = Resources.FindObjectsOfTypeAll<GizmoHandle>();
        int appliedCount = 0;

        foreach (GizmoHandle handle in handles)
        {
            if (handle.Kind != GizmoHandleKind.Rotate)
            {
                continue;
            }

            if (EditorUtility.IsPersistent(handle))
            {
                continue;
            }

            ApplyRingMesh(handle.gameObject, ringMesh);
            appliedCount++;
        }

        if (appliedCount == 0)
        {
            Debug.LogWarning("씬에서 Rotate GizmoHandle을 찾지 못했습니다.");
            return;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"{appliedCount}개 Rotate 핸들에 ring 메시를 적용했습니다. Scene view에서 각 링의 방향(회전)을 확인하고 필요하면 조정한 뒤 씬을 저장하세요.");
    }

    private static void ApplyRingMesh(GameObject handle, Mesh ringMesh)
    {
        MeshFilter meshFilter = handle.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = ringMesh;

        SphereCollider oldCollider = handle.GetComponent<SphereCollider>();

        if (oldCollider != null)
        {
            Object.DestroyImmediate(oldCollider, true);
        }

        MeshCollider meshCollider = handle.GetComponent<MeshCollider>();

        if (meshCollider == null)
        {
            meshCollider = handle.AddComponent<MeshCollider>();
        }

        // convex를 켜면 토러스의 빈 구멍이 볼록 껍질로 메워져 클릭 판정이 부정확해지므로,
        // 정적 콜라이더(레이캐스트 전용)에는 non-convex를 그대로 사용해 실제 링 형태를 유지.
        meshCollider.convex = false;
        meshCollider.sharedMesh = ringMesh;

        EditorUtility.SetDirty(handle);
    }
}
