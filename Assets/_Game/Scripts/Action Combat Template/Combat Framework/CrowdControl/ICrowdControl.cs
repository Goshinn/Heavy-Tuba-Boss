using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ICrowdControl
{
    bool IsCrowdControlled { get; }

    // Events
    event Action CrowdControlled;
}
