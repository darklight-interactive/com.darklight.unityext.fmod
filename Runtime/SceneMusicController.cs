using System.Collections;
using Darklight.Editor;
using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Darklight.FMODExt
{
    public class SceneMusicController : MonoBehaviour
    {
        private EventInstance _currentSongInstance;
        
        [SerializeField, ReadOnly]
        private EventReference _currentSongEventRef;
        
        [HorizontalLine]
        [SerializeField, Expandable]
        [CreateAsset("NewSceneMusicLibrary", FMODManager.RESOURCE_PATH)]
        protected SceneMusicLibrary sceneMusicLibrary;

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (_currentSongInstance.isValid())
            {
                _currentSongInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _currentSongInstance.release();
                _currentSongInstance.clearHandle();
            }
        }
        
        void SetEventInEmitter(EventReference newSongEventRef)
        {
            Debug.Log("Playing new song: " + newSongEventRef, this);
            
            if (_currentSongInstance.isValid())
            {
                _currentSongInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _currentSongInstance.release();
                _currentSongInstance.clearHandle();
            }
            
            // << CREATE NEW INSTANCE >>
            _currentSongInstance = RuntimeManager.CreateInstance(newSongEventRef);

            // Check if the event is 3D
            EventDescription description = RuntimeManager.GetEventDescription(newSongEventRef);
            description.is3D(out bool is3D);
            if (is3D)
            {
                Debug.LogWarning(
                    $"FMOD Event '{newSongEventRef.Path}' is a 3D event. " +
                    "Music events should typically be 2D. Either attach the instance to a GameObject " +
                    "or change the event to 2D in FMOD Studio.",
                    this);
                RuntimeManager.AttachInstanceToGameObject(_currentSongInstance, transform);
            }

            // << START INSTANCE >>
            _currentSongInstance.start();
            _currentSongEventRef = newSongEventRef;
        }
        
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded: " + scene.name, this);
            if (sceneMusicLibrary == null)
            {
                Debug.LogWarning("SceneMusicLibrary is not assigned.", this);
                return;
            }

            EventReference songEventRef = sceneMusicLibrary.GetBackgroundMusicByScene(scene.name);
            if (!songEventRef.IsNull)
            {
                PlaySong(songEventRef);
            }
        }

        bool IsPlaying()
        {
            _currentSongInstance.getPlaybackState(out PLAYBACK_STATE playbackState);
            return playbackState != PLAYBACK_STATE.STOPPED;
        }
        
        bool IsPlayingSong(EventReference newSongEventRef)
        {
            if (!IsPlaying())
                return false;
            return !_currentSongEventRef.IsNull && _currentSongEventRef.Guid == newSongEventRef.Guid;
        }


        
        [Button]
        public void PlaySongForCurrentScene()
        {
            EventReference songEventRef = sceneMusicLibrary.GetBackgroundMusicByScene(SceneManager.GetActiveScene().name);
            if (!songEventRef.IsNull)
            {
                PlaySong(songEventRef);
            }
        }
        
        public void PlaySong(EventReference newSongEventRef)
        {
            if (newSongEventRef.IsNull)
            {
                Debug.LogWarning($"FMOD SONG event path does not exist: " + newSongEventRef, this);
                return;
            }

            // Check if the same song is already playing
            if (IsPlayingSong(newSongEventRef))
            {
                Debug.Log("Already playing song: " + newSongEventRef, this);
                return;
            }

            // If currently playing a different song, transition
            if (_currentSongInstance.isValid())
            {
                StartCoroutine(EventTransitionRoutine(newSongEventRef));
                return;
            }

            SetEventInEmitter(newSongEventRef);
        }

        public void StopSong()
        {
            StartCoroutine(StopCurrentSongRoutine());
        }
        
        IEnumerator EventTransitionRoutine(EventReference newSongEventRef)
        {
            if (newSongEventRef.IsNull)
            {
                Debug.LogWarning($"FMOD SONG event path does not exist: " + newSongEventRef, this);
                yield break;
            }

            // Begin fading out the current song
            _currentSongInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //Debug.Log("Fading out current song.", this);

            // Wait for the song to stop playing
            PLAYBACK_STATE playbackState = FMODManager.GetPlaybackState(_currentSongInstance);
            while (playbackState != PLAYBACK_STATE.STOPPED)
            {
                //Debug.Log("Waiting for song to stop playing. " + playbackState, this);
                _currentSongInstance.getPlaybackState(out playbackState);
                yield return null;
            }

            SetEventInEmitter(newSongEventRef);
        }

        IEnumerator StopCurrentSongRoutine()
        {
            Debug.Log(_currentSongInstance);
            _currentSongInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            PLAYBACK_STATE playbackState = FMODManager.GetPlaybackState(_currentSongInstance);
            while (playbackState != PLAYBACK_STATE.STOPPED)
            {
                _currentSongInstance.getPlaybackState(out playbackState);
                yield return null;
            }

            _currentSongInstance.release();
        }
    }
}