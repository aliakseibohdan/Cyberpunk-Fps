using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

#nullable enable

public enum DisplayType
{
    TerminalText,
    Dialogue,
    AnimatedSprite
}

[Serializable]
public struct TerminalLine
{
    public string Text;
    public float PreLineDelay;
    public Color Color;
    public AudioClip? LineSpecificSound;
}

[Serializable]
public struct DialogueLine
{
    public string SpeakerName;
    public string Text;
    public float PreLineDelay;
    public AudioClip? VoiceLine;
    public Sprite? SpeakerPortrait;
}

[Serializable]
public struct SpriteAnimation
{
    public Sprite[] Frames;
    public float FrameRate;
    public AudioClip[]? FrameSounds;
    public float LifetimeAfterAnimation;
}