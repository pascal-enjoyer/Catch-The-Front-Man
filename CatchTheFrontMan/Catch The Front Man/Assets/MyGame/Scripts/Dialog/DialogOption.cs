using System;
using System.Collections.Generic;
using UnityEngine.Events;


[Serializable]
public class DialogOption
{
    public string Text;
    public string NextNodeId;
    public UnityEvent OnSelect;
}