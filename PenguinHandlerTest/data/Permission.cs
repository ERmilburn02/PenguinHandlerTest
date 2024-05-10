using System;
using System.Collections.Generic;

namespace PenguinHandlerTest;

/// <summary>
/// Registered server permissions
/// </summary>
public partial class Permission
{
    /// <summary>
    /// Unique permission identifier
    /// </summary>
    public string Name { get; set; }

    public bool Enabled { get; set; }

    public virtual ICollection<Penguin> Penguins { get; set; } = new List<Penguin>();
}
