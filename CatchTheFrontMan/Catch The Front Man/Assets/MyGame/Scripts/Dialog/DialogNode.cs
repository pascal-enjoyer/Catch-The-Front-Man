using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

[Serializable]
public class DialogNode
{
    public string NodeId;
    [TextArea] public string Text;
    public List<DialogOption> Options;
    public UnityEvent OnNodeEnter;
    public Sprite PlayerAvatar; // Спрайт аватарки игрока для этого узла
    public Sprite InterlocutorAvatar; // Спрайт аватарки собеседника для этого узла
}