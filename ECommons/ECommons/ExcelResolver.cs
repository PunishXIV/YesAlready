using Dalamud;
using Lumina.Excel;
using System;
using System.Collections.Generic;
using ECommons.DalamudServices;

#nullable enable
namespace ECommons;

/// <summary>
/// This object resolves a rowID within an Excel sheet.
/// </summary>
/// <typeparam name="T">The type of Lumina sheet to resolve.</typeparam>
public class ExcelResolver<T> : IEquatable<ExcelResolver<T>> where T : ExcelRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelResolver{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the classJob.</param>
    public ExcelResolver(uint id)
    {
        this.Id = id;
    }

    public ExcelResolver(Dalamud.Game.ClientState.Resolvers.ExcelResolver<T> dalamudResolver)
    {
        this.Id = dalamudResolver.Id;
    }

    /// <summary>
    /// Gets the ID to be resolved.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets GameData linked to this excel row.
    /// </summary>
    public T? GameData => Svc.Data.GetExcelSheet<T>()?.GetRow(this.Id);

    public override bool Equals(object? obj)
    {
        return obj is ExcelResolver<T> resolver &&
               Id == resolver.Id;
    }
    public bool Equals(ExcelResolver<T>? resolver)
    {
        return resolver != null && Id == resolver.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    /// <summary>
    /// Gets GameData linked to this excel row with the specified language.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <returns>The ExcelRow in the specified language.</returns>
    public T? GetWithLanguage(ClientLanguage language) => Svc.Data.GetExcelSheet<T>(language)?.GetRow(this.Id);

    public static bool operator ==(ExcelResolver<T>? left, ExcelResolver<T>? right)
    {
        return EqualityComparer<ExcelResolver<T>>.Default.Equals(left, right);
    }

    public static bool operator !=(ExcelResolver<T>? left, ExcelResolver<T>? right)
    {
        return !(left == right);
    }

    public bool ShouldSerializeGameData()
    {
        return false;
    }
}

public static class ExcelResolverExtensions
{
    public static ExcelResolver<T> ToECommons<T>(this Dalamud.Game.ClientState.Resolvers.ExcelResolver<T> dalamudResolver) where T : ExcelRow
    {
        return new ExcelResolver<T>(dalamudResolver);
    }
}
