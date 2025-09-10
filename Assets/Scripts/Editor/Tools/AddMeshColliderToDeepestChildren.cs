using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AddMeshColliderToDeepestChildren : MonoBehaviour
{
    [MenuItem("GameObject/Add Mesh Colliders to Deepest Children", false, 49)]
    static void AddMeshCollidersToDeepestChildren()
    {
        foreach (GameObject selectedGameObject in Selection.gameObjects)
        {
            AddColliderToDeepestChildren(selectedGameObject.transform);
        }
    }

    [MenuItem("GameObject/Add Mesh Colliders to Deepest Children", true)]
    static bool ValidateAddMeshCollidersToDeepestChildren()
    {
        return Selection.activeGameObject != null;
    }

    static void AddColliderToDeepestChildren(Transform parentTransform)
    {
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(parentTransform);

        while (stack.Count > 0)
        {
            Transform current = stack.Pop();

            if (current.childCount == 0)
            {
                if (current.GetComponent<MeshCollider>() == null)
                {
                    MeshFilter meshFilter = current.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        Undo.AddComponent<MeshCollider>(current.gameObject);
                        Debug.Log($"Added MeshCollider to deepest child: {current.name}", current);
                    }
                }
            }
            else
            {
                foreach (Transform child in current)
                {
                    stack.Push(child);
                }
            }
        }
    }
}