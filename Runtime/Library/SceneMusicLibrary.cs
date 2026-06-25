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

        public EventReference GetBackgroundMusicByScene(string sceneName) => _sceneEventReferences[sceneName];
    }
}