using System;
using System.Collections.Generic;

namespace PenguinHandlerTest;

/// <summary>
/// Penguin activation keys
/// </summary>
public partial class ActivationKey
{
    /// <summary>
    /// Penguin ID
    /// </summary>
    public int PenguinId { get; set; }

    /// <summary>
    /// Penguin activation key
    /// </summary>
    public string ActivationKey1 { get; set; }

    public virtual Penguin Penguin { get; set; }
}
