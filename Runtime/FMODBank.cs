using System.Collections.Generic;
using UnityEngine;
using Darklight.Editor;
using System.Linq;


/// <summary>
/// A serializable class that mannages and stores FMOD bank data
/// </summary>
[System.Serializable]
public class FMODBank
{
    private FMOD.GUID _guid;
    private FMOD.Studio.Bank _bank;
    private FMOD.Studio.EventDescription[] _events;
    private FMOD.Studio.Bus[] _buses;
    private List<FMODBus> _busData;
    public List<FMODBus> BusData => _busData;
    bool initialized;

    [SerializeField, ShowOnly] string _path;
    public string Path => _path;

    [Header("Buses")]
    [SerializeField, ShowOnly] int _busCount;
    [SerializeField, ShowOnly] FMOD.RESULT _busListOk = FMOD.RESULT.ERR_UNIMPLEMENTED;
    [SerializeField, ShowOnly] List<string> _busPaths = new();

    [Header("Events")]
    [SerializeField, ShowOnly] int _eventCount;

    // --------------------- [[ CONSTRUCTORS ]] --------------------- //
    public FMODBank(FMOD.Studio.Bank bank)
    {
        _bank = bank;
        initialized = true;

        // Load Identifiers
        bank.getID(out _guid);
        bank.getPath(out _path);

        // Load Buses
        bank.getBusCount(out _busCount);
        _busListOk = bank.getBusList(out _buses);
        _busPaths = _buses.ToList().ConvertAll(b => b.getPath(out string path) == FMOD.RESULT.OK ? path : "ERROR");
        _busData = _buses.ToList().ConvertAll(b => new FMODBus(b));

        // Load Events
        bank.getEventCount(out _eventCount);
        bank.getEventList(out _events);
    }

    public void UnloadBank()
    {
        if (initialized)
        {
            _bank.unload();
            _events.ToList().Clear();
            _buses.ToList().Clear();
            initialized = false;
            Debug.Log($"Bank {_path} unloaded successfully.");
        }
        else
        {
            Debug.LogWarning($"Bank {_path} is not loaded.");
        }
    }
}
