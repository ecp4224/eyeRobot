using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEvent {
    
    /// <summary>
    /// A unique ID that represents this TYPE of event.
    /// </summary>
    byte EventId { get; }
}
