#if UNITY_EDITOR
using Darklight.Editor;
using UnityEditor;

namespace Darklight.FMODExt.Editor
{

    [CustomEditor(typeof(FMODManager), true)]
    public class FMODManagerEditor : UnityEditor.Editor
    {
        SerializedObject _serializedObject;
        FMODManager _script;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(target);
            _script = (FMODManager)target;
        }

        public override void OnInspectorGUI()
        {
            _serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            // Draw the consoleGUI in the inspector
            FMODManager.InternalConsole.DrawInEditor();

            CustomInspectorGUI.DrawDefaultInspectorWithoutScript(target);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif