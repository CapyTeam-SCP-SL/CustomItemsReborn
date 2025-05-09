// -----------------------------------------------------------------------
// <copyright file="Items.cs" company="Joker119">
// Copyright (c) Joker119. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable SA1200

using CustomItemsReborn.Items;

namespace CustomItemsReborn.Configs;

using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// All item config settings.
/// </summary>
public class Items
{
    /// <summary>
    /// Gets the HashSet of emp greanades.
    /// </summary>
    [Description("The HashSet of EMP grenades.")]
    public HashSet<EmpGrenade> EmpGrenades { get; private set; } = new()
    {
        new EmpGrenade(),
    };

    /// <summary>
    /// Gets the HashSet of grenade launchers.
    /// </summary>
    [Description("The HashSet of grenade launchers.")]
    public HashSet<GrenadeLauncher> GrenadeLaunchers { get; private set; } = new()
    {
        new GrenadeLauncher(),
    };

    /// <summary>
    /// Gets the HashSet of implosion grenades.
    /// </summary>
    [Description("The HashSet of implosion grenades.")]
    public HashSet<ImplosionGrenade> ImplosionGrenades { get; private set; } = new()
    {
        new ImplosionGrenade(),
    };

    /// <summary>
    /// Gets the HashSet of lethal injections.
    /// </summary>
    [Description("The HashSet of lethal injections.")]
    public HashSet<LethalInjection> LethalInjections { get; private set; } = new()
    {
        new LethalInjection(),
    };

    /// <summary>
    /// Gets the HashSet of lucky coins.
    /// </summary>
    [Description("The HashSet of lucky coins.")]
    public HashSet<LuckyCoin> LuckyCoins { get; private set; } = new()
    {
        new LuckyCoin(),
    };

    /// <summary>
    /// Gets the HashSet of mediGuns.
    /// </summary>
    [Description("The HashSet of mediGuns.")]
    public HashSet<MediGun> MediGuns { get; private set; } = new()
    {
        new MediGun(),
    };
    /// <summary>
    /// Gets the HashSet of Scp1499s.
    /// </summary>
    [Description("The HashSet of Scp1499s.")]
    public HashSet<Scp1499> Scp1499s { get; private set; } = new()
    {
        new Scp1499(),
    };

    /// <summary>
    /// Gets the HashSet of sniper rifles.
    /// </summary>
    [Description("The HashSet of sniper rifles.")]
    public HashSet<SniperRifle> SniperRifle { get; private set; } = new()
    {
        new SniperRifle(),
    };

    /// <summary>
    /// Gets the HashSet of tranquilizer guns.
    /// </summary>
    [Description("The HashSet of tranquilizer guns.")]
    public HashSet<TranquilizerGun> TranquilizerGun { get; private set; } = new()
    {
        new TranquilizerGun(),
    };

    /// <summary>
    /// Gets the HashSet of Scp714s.
    /// </summary>
    [Description("The HashSet of Scp714s.")]
    public HashSet<Scp714> Scp714s { get; private set; } = new()
    {
        new Scp714(),
    };

    /// <summary>
    /// Gets the HashSet of Anti-Memetic Pills.
    /// </summary>
    [Description("The HashSet of Anti-Memetic Pills.")]
    public HashSet<AntiMemeticPills> AntiMemeticPills { get; private set; } = new()
    {
        new AntiMemeticPills(),
    };

    /// <summary>
    /// Gets the HashSet of DeflectorSheilds.
    /// </summary>
    [Description("The HashSet of DeflectorSheilds.")]
    public HashSet<DeflectorShield> DeflectorSheilds { get; private set; } = new()
    {
        new DeflectorShield(),
    };

    /// <summary>
    /// Gets the HashSet of <see cref="Scp2818"/>s.
    /// </summary>
    [Description("The HashSet of SCP-2818s.")]
    public HashSet<Scp2818> Scp2818s { get; private set; } = new()
    {
        new Scp2818(),
    };

    /// <summary>
    /// Gets the HashSet of AutoGuns.
    /// </summary>
    [Description("The HashSet of AutoGuns.")]
    public HashSet<AutoGun> AutoGuns { get; private set; } = new()
    {
        new AutoGun(),
    };
}