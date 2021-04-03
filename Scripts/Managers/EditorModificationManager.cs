using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Managers
{
    [ExecuteInEditMode]
    public class EditorModificationManager : AbstractSingleton<EditorModificationManager>
    {
        [Header("Tags")]
        [SerializeField] private int _numTagsToGenerate;
        [SerializeField] private string _tagTemplate;
        [SerializeField] private bool _generateTags;

        [Header("GameObjects")]
        [SerializeField] private bool _setTags;

        [Header("Selection")]
        [SerializeField] private string _selectionTag;
        [SerializeField] private bool _select;

        protected override void Initialize()
        {

        }

        private void SetTags()
        {
            var currentTag = 0;
            for (var i = 0; i < _numTagsToGenerate; i++)
            {
                var objects = GameObject.FindGameObjectsWithTag($"G_{i}");

                if (objects.Any())
                {
                    foreach (var go in objects)
                    {
                        go.tag = $"G_{currentTag}";
                    }

                    currentTag++;
                }
            }

            Debug.Log($"Last tag G_{currentTag}");
        }

        private void SelectWithTag()
        {
            var gameObjects = GameObject.FindGameObjectsWithTag(_selectionTag);
            Selection.objects = gameObjects;
        }

        private void Update()
        {
            if (_generateTags)
            {
                GenerateTags();
                _generateTags = false;
            }

            if (_setTags)
            {
                SetTags();
                _setTags = false;
            }

            if (_select)
            {
                SelectWithTag();
                _select = false;
            }
        }

        private void GenerateTags()
        {
            var listToGenerate = new List<string>();
            for (var i = 0; i < _numTagsToGenerate; i++)
            {
                listToGenerate.Add($"{_tagTemplate}_{i}");
            }

            EditorUtils.AddTags(listToGenerate);
        }
    }
}
