using System.Collections;
using System.Collections.Generic;
using Darklight.Behaviour;
using Darklight.Editor;
using Darklight;
using Darklight.Utility;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Darklight.FMODExt
{
    /// <summary>
    /// This is the main singleton class that loads all FMOD audio events and buses.
    /// This class also provides static methods for handling FMOD components.
    /// </summary>
    public class FMODManager : MonoBehaviourSingleton<FMODManager>
    {
        public const string RESOURCE_PATH = AssetUtility.RESOURCE_FILEPATH + "/FMODExt";
        
        Coroutine _loadingCoroutine;
        Coroutine _repeatedEventCoroutine;
        
        public static ConsoleGUI InternalConsole { get; private set; } = new ConsoleGUI();
        
        [Header("(( ---- FMOD BANKS ---- ))")]
        [SerializeField, ShowOnly]
        FMOD.RESULT _bankLoadResult;

        [SerializeField]
        List<FMODBank> _bankData;

        [Header("(( ---- FMOD BUSES ---- ))")]
        [SerializeField, ShowOnly]
        FMOD.RESULT _busLoadResult;

        [SerializeField]
        List<FMODBus> _busData;
        
        #region == [[ MONOBEHAVIOUR SINGLETON METHODS ]] ==================================== >>

        void Awake()
        {
            LoadBanksAndBuses();
        }
        
        void Update()
        {
            // << UPDATE BUS DATA >>
            foreach (FMODBus bus in _busData)
            {
                bus.Update();
            }
        }
        #endregion
        
        #region == < METHODS > [ HANDLE BANKS AND BUSES ] ================================================ >>>

        IEnumerator LoadBanksAndBusesRoutine()
        {
            // ------- Load Banks ------- //
            RuntimeManager.StudioSystem.getBankList(out Bank[] banks);
            InternalConsole.Log($"Loading Banks. Count: {banks.Length}");
            foreach (Bank bank in banks)
            {
                // Load the bank
                _bankLoadResult = bank.loadSampleData();
                if (_bankLoadResult == FMOD.RESULT.OK)
                {
                    FMODBank newBankData = new FMODBank(bank);
                    _bankData.Add(newBankData);
                    InternalConsole.Log(
                        $"Bank Load Result: " + newBankData.Path + " -> " + _bankLoadResult,
                        1
                    );
                }
            }

            // ------- Load Buses ------- //
            InternalConsole.Log($"Loading Buses.");
            foreach (FMODBank bank in _bankData)
            {
                List<FMODBus> busData = bank.BusData;
                _busData.AddRange(busData);
                foreach (FMODBus bus in busData)
                {
                    InternalConsole.Log(
                        $"Bus Load Result: " + bus.Path + " -> " + bus.LoadResult,
                        1
                    );
                }
            }
            
            Debug.Log("FMOD Manager Loaded. Banks: " + _bankData.Count + ", Buses: " + _busData.Count + "");
            yield return null;
        }
        
        protected void LoadBanksAndBuses()
        {
            if (_loadingCoroutine != null)
                StopCoroutine(_loadingCoroutine);
            _loadingCoroutine = StartCoroutine(LoadBanksAndBusesRoutine());
        }
        #endregion

        #region == < STATIC_METHODS > [[ GETTER FUNCTIONS ]] =============================================== >>

        /// Retrieves the playback state of the specified EventInstance.
        /// <param name="instance">The EventInstance whose playback state is to be retrieved.</param>
        /// <return>The current playback state of the EventInstance as a PLAYBACK_STATE enumerator.</return>
        public static PLAYBACK_STATE GetPlaybackState(EventInstance instance)
        {
            PLAYBACK_STATE pS;
            instance.getPlaybackState(out pS);
            return pS;
        }

        /// Retrieves the event path of the specified EventInstance.
        /// <param name="instance">The EventInstance whose event path is to be retrieved.</param>
        /// <return>The event path of the specified EventInstance as a string in the form event:/folder/sub-folder/eventName.</return>
        public static string GetInstantiatedEventPath(EventInstance instance)
        {
            string result;
            EventDescription description;

            instance.getDescription(out description);
            description.getPath(out result);

            // expect the result in the form event:/folder/sub-folder/eventName
            return result;
        }
        #endregion
        
        #region == < STATIC_METHODS > [[ PLAY EVENT FUNCTIONS ]] =============================================== >>

        /// Plays a one-shot FMOD audio event at the default 3D position.
        /// <param name="eventReference">The EventReference representing the audio event to be played.</param>
        public static void PlayOneShot(EventReference eventReference)
        {
            RuntimeManager.PlayOneShot(eventReference);
        }

        /// Plays an FMOD event with the specified parameters.
        /// <param name="eventReference">The EventReference of the FMOD event to play.</param>
        /// <param name="parameters">A variable number of tuples, where each tuple specifies a parameter name and its corresponding value to be set for the event.</param>
        public static void PlayEventWithParameters(
            EventReference eventReference,
            params (string name, float value)[] parameters
        )
        {
            if (eventReference.IsNull)
                return;
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            foreach (var (name, value) in parameters)
            {
                instance.setParameterByName(name, value);
            }
            instance.start();
            instance.release();
        }

        /// Plays an FMOD event with specified string parameters by name.
        /// <param name="eventReference">The reference to the FMOD event to play.</param>
        /// <param name="parameters">An array of parameter name and string label pairs to set for the event instance before it starts.</param>
        public static void PlayEventWithParametersByName(
            EventReference eventReference,
            params (string name, string value)[] parameters
        )
        {
            if (eventReference.IsNull)
                return;
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            foreach (var (name, value) in parameters)
            {
                instance.setParameterByNameWithLabel(name, value);
            }
            instance.start();
            instance.release();
        }
        #endregion
        
        // Coroutine to handle the repeated playing of an event
        IEnumerator RepeatEventRoutine(EventReference eventReference, float interval)
        {
            while (true)
            {
                FMODManager.PlayOneShot(eventReference);
                yield return new WaitForSeconds(interval);
            }
        }
        
        // Method to start repeating an event
        public void StartRepeatingEvent(EventReference eventReference, float interval)
        {
            _repeatedEventCoroutine = StartCoroutine(RepeatEventRoutine(eventReference, interval));
        }

        // Method to stop repeating an event
        public void StopRepeatingEvent()
        {
            StopCoroutine(_repeatedEventCoroutine);
        }

    }
    
}
