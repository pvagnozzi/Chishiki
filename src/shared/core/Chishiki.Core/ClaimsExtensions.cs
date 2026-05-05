// -----------------------------------------------------------------------------
// File:        ClaimsExtensions.cs
// Author:      Piergiorgio Vagnozzi
// Description: Extension methods for claims manipulation, merging, and conversion to dictionary.
// Created:     2024-04-15
// Modified:    2026-05-04
// -----------------------------------------------------------------------------
// Copyright (c) Piergiorgio Vagnozzi. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// -----------------------------------------------------------------------------
using System.Security.Claims;

namespace Chishiki;

/// <summary>
/// Claims Extensions.
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>Converts a collection of claims to a dictionary keyed by claim type.</summary>
    /// <param name="claims">Claims.</param>
    /// <returns>Dictionary.</returns>
    public static Dictionary<string, string> ToDictionary(this IEnumerable<Claim> claims) =>
        claims.ToDictionary(x => x.Type, x => x.Value);

    /// <summary>
    /// Gets the claims dictionary from the claims principal.
    /// </summary>
    /// <param name="claimPrincipal">Claims principal.</param>
    /// <returns>Claims dictionary.</returns>
    public static Dictionary<string, string> ToClaimDictionary(this ClaimsPrincipal claimPrincipal) =>
        claimPrincipal.Claims.ToDictionary();

    /// <summary>
    /// Adds the specified claim if not exists.
    /// </summary>
    /// <param name="claims">Clamis.</param>
    /// <param name="claim">Claims to add</param>
    /// <returns>Claim result.</returns>
    public static IEnumerable<Claim> AddMissing(this IEnumerable<Claim> claims, Claim claim)
    {
        if (claims.Any(c => c.Type == claim.Type))
        {
            return claims;
        }
        var list = claims.ToList();
        list.Add(claim);
        return list;
    }

    /// <summary>
    /// Adds the specified claim if not existing claim of the same type.
    /// </summary>
    /// <param name="claims">Source claims</param>
    /// <param name="newClaims">New claims.</param>
    /// <returns>Merged claims.</returns>
    public static List<Claim> AddMissingRange(this List<Claim> claims, Claim[] newClaims)
    {
        var list = claims;

        foreach (var claim in newClaims)
        {
            if (list.Any(c => c.Type == claim.Type))
            {
                continue;
            }
            list.Add(claim);
        }        
        return list;
    }
}
