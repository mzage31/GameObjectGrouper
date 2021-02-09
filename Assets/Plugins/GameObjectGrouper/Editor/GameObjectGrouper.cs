using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityHierarchyFolders.Runtime;

namespace GameObjectGrouper
{
    public static class GameObjectGrouper
    {
        private static double _renameTime;
        
        [MenuItem("GameObject/MakeGroup %g")]
        public static void MakeGroup()
        {
            var selectedObjects = Selection.gameObjects.Select(x => x.transform).ToList();
            if (selectedObjects.Count == 0)
                return;
            var first = selectedObjects.First();
            var allHaveSameParents = selectedObjects.All(x => x.parent == first.parent);

            var parentObject = new GameObject("Group");
            parentObject.transform.position = new Vector3(
                selectedObjects.Select(x => x.position.x).Average(),
                selectedObjects.Select(x => x.position.y).Average(),
                selectedObjects.Select(x => x.position.z).Average()
            );
            if (allHaveSameParents)
            {
                parentObject.transform.SetParent(first.parent);
                parentObject.transform.SetSiblingIndex(first.GetSiblingIndex());
            }
            Undo.RegisterCreatedObjectUndo(parentObject, "Group created");

            foreach (var obj in selectedObjects)
                Undo.SetTransformParent(obj, parentObject.transform, "Changed Parent");

            EditorApplication.update += EngageRenameMode;
            _renameTime = EditorApplication.timeSinceStartup + 0.25d;
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            Selection.activeGameObject = parentObject;
        }

        private static void EngageRenameMode()
        {
            if (EditorApplication.timeSinceStartup < _renameTime)
                return;
            
            EditorApplication.update -= EngageRenameMode;
            var e = new Event { keyCode = KeyCode.F2, type = EventType.KeyDown };
            EditorWindow.focusedWindow.SendEvent(e);
        }
    }
}

            