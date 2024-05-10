﻿using System;
using System.Collections.Generic;

namespace PenguinHandlerTest;

/// <summary>
/// Server location crumbs
/// </summary>
public partial class Location
{
    /// <summary>
    /// Unique location ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Location name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Cost of location
    /// </summary>
    public int Cost { get; set; }

    /// <summary>
    /// Is location patched?
    /// </summary>
    public bool Patched { get; set; }

    /// <summary>
    /// Add to default legacy inventory?
    /// </summary>
    public bool LegacyInventory { get; set; }

    /// <summary>
    /// Add to default vanilla inventory?
    /// </summary>
    public bool VanillaInventory { get; set; }

    public virtual ICollection<PenguinIglooRoom> PenguinIglooRooms { get; set; } = new List<PenguinIglooRoom>();

    public virtual ICollection<RedemptionCode> Codes { get; set; } = new List<RedemptionCode>();

    public virtual ICollection<Penguin> Penguins { get; set; } = new List<Penguin>();
}
