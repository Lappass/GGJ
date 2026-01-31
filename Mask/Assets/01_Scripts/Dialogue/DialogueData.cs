using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speaker;

    [TextArea(2, 6)]
    public string content;
}

[Serializable]
public class DialogueSequence
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}
