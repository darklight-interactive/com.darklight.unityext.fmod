using System.Collections;
using Darklight.Editor;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Darklight.FMODExt
{
    public class SceneMusicController : MonoBehaviour
    {
        public static EventInstance CurrentSongInstance { get; private set; }
        public static EventDescription CurrentSongDescription { get; private set; }
        
        [CreateAsset("NewSceneMusicLibrary", FMODManager.RESOURCE_PATH)]
        protected SceneMusicLibrary sceneMusicLibrary;
        
        public void PlaySong(EventReference newSongEventRef)
        {
            if (newSongEventRef.IsNull)
            {
                Debug.LogWarning($"FMOD SONG event path does not exist: " + newSongEventRef, this);
                return;
            }

            // Retrieve the GUID of the current and new song events
            CurrentSongInstance.getDescription(out EventDescription eventDescription);
            eventDescription.getID(out FMOD.GUID currentEventGuid);

            FMOD.GUID newEventGuid = newSongEventRef.Guid;

            // Compare the GUIDs
            if (currentEventGuid == newEventGuid)
            {
                Debug.LogWarning($"FMOD SONG event is already playing: " + newSongEventRef, this);
                return;
            }

            // If the current background music is playing, fade it out and start the new song
            if (
                CurrentSongInstance.isValid()
                && FMODManager.GetPlaybackState(CurrentSongInstance) == PLAYBACK_STATE.PLAYING
            )
            {
                StartCoroutine(EventTransitionRoutine(newSongEventRef));
                return;
            }

            // Create a new instance of the song and start it
            EventInstance newSongInstance = RuntimeManager.CreateInstance(newSongEventRef);
            CurrentSongInstance = newSongInstance;
            newSongInstance.start();
            newSongInstance.release();
        }

        IEnumerator EventTransitionRoutine(EventReference newSongEventRef)
        {
            if (newSongEventRef.IsNull)
            {
                Debug.LogWarning($"FMOD SONG event path does not exist: " + newSongEventRef, this);
                yield break;
            }

            // Begin fading out the current song
            CurrentSongInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            // Wait for the song to stop playing
            PLAYBACK_STATE playbackState = FMODManager.GetPlaybackState(CurrentSongInstance);
            while (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                CurrentSongInstance.getPlaybackState(out playbackState);
                yield return null;
            }

            // Create a new instance of the song and start it
            EventInstance newSongInstance = RuntimeManager.CreateInstance(newSongEventRef);
            CurrentSongInstance = newSongInstance;
            newSongInstance.start();
            newSongInstance.release();
        }

        public void StopCurrentSong()
        {
            StartCoroutine(StopCurrentSongRoutine());
        }

        public IEnumerator StopCurrentSongRoutine()
        {
            Debug.Log(CurrentSongInstance);
            CurrentSongInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            PLAYBACK_STATE playbackState = FMODManager.GetPlaybackState(CurrentSongInstance);
            while (playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED)
            {
                CurrentSongInstance.getPlaybackState(out playbackState);
                yield return null;
            }

            CurrentSongInstance.release();
        }
    }
}