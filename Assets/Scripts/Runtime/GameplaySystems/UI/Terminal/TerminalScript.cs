using UnityEngine;

[CreateAssetMenu(fileName = "New Terminal Script", menuName = "Terminal System/Terminal Script")]
public class TerminalScript : ScriptableObject
{
    [TextArea(10, 20)]
    public string scriptText;

    public float defaultDelay = 0.5f;
    public Color defaultColor = Color.green;
    public AudioClip defaultTypeSound;
}