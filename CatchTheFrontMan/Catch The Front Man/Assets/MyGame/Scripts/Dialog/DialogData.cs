using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogData", menuName = "Dialog/DialogData")]
public class DialogData : ScriptableObject
{
    public string StartingNodeId;
    public List<DialogNode> Nodes;
}