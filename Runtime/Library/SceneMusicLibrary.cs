using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Darklight.Editor;
using FMODUnity;
using UnityEngine;

namespace Darklight.FMODExt
{
    [CreateAssetMenu(menuName = "Darklight/FMODExt/MusicObject")]
    public class SceneMusicLibrary : ScriptableObject
    {
        [SerializeField]
        private SerializedDictionary<SceneObject, EventReference> _sceneEventReferences = new SerializedDictionary<SceneObject, EventReference>();

        public EventReference GetBackgroundMusicByScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return new EventReference();
            
            return _sceneEventReferences.TryGetValue(sceneName, out var eventRef) 
                ? eventRef 
                : new EventReference();
        }
        
        public string PrintSceneNames()
        {
            string result = "";
            foreach (KeyValuePair<SceneObject, EventReference> kvp in _sceneEventReferences)
            {
                result += kvp.Key + ": " + kvp.Value + "\n";
            }
            return result;
        }
        
    }
}