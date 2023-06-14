using _2D_WorldGen.Scripts.GenerationTree;
using _2D_WorldGen.Scripts.GenerationTree.Core;
using UnityEditor;
using UnityEngine;

namespace _2D_WorldGen.Editor.GenerationTree.Core
{
    public class GenerationAlgorithmEditorWindow : EditorWindow
    {
        private GenerationAlgorithm _rootAlgorithm;

        [MenuItem("Window/Generation Algorithm Tree")]
        public static void OpenWindow()
        {
            var window = GetWindow<GenerationAlgorithmEditorWindow>();
            window.titleContent = new GUIContent("Generation Algorithm Tree");
            window.Show();
        }

        private void OnGUI()
        {
            _rootAlgorithm = EditorGUILayout.ObjectField("Root Algorithm", 
                _rootAlgorithm, typeof(GenerationAlgorithm), false) as GenerationAlgorithm;

            if (_rootAlgorithm != null)
            {
                EditorGUILayout.LabelField(_rootAlgorithm.name, EditorStyles.boldLabel);
                foreach (var child in _rootAlgorithm.dependencies)
                {
                    DrawAlgorithmElement(child);
                }
            }
        }

        private static void DrawAlgorithmElement(GenerationAlgorithm algorithm)
        {
            EditorGUILayout.LabelField("\u2514\u2500 " + algorithm.name);

            EditorGUI.indentLevel++;

            if (algorithm.dependencies != null)
            {
                foreach (var dependency in algorithm.dependencies)
                {
                    DrawAlgorithmElement(dependency);
                }
            }

            EditorGUI.indentLevel--;
        }
    }

}