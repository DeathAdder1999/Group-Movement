using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utility
{
    public static class EditorUtils
    {
        public static void AddTags(IEnumerable<string> tags)
        {
            // Open tag manager
            var tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");

            // Adding a Tag
            foreach (var tag in tags)
            {
                // First check if it is not already present
                var found = false;
                for (var i = 0; i < tagsProp.arraySize; i++)
                {
                    var t = tagsProp.GetArrayElementAtIndex(i);

                    if (t.stringValue.Equals(tag))
                    {
                        found = true;
                        break;
                    }
                }

                // if not found, add it
                if (!found)
                {
                    Debug.Log("Adding");
                    tagsProp.InsertArrayElementAtIndex(0);
                    var n = tagsProp.GetArrayElementAtIndex(0);
                    n.stringValue = tag;
                }
            }

            tagManager.ApplyModifiedProperties();
        }
    }
}
