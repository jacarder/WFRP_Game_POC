using UnityEngine;

[CreateAssetMenu(fileName = "EncounterScriptable", menuName = "ScriptableObject/EncounterScriptable", order = 0)]
public class EncounterScriptable : ScriptableObject {
    public string type = "Beastmen";
    public int dangerLevel = 5;
}