﻿namespace Dapplo.Confluence.Query;

/// <summary>
///     A clause which cannot be modified anymore, only ToString() makes sense
/// </summary>
public interface IFinalClause
{
    /// <summary>
    ///     Specify the order by field, default field order is used, this can be called mutiple times
    /// </summary>
    /// <param name="field">Field to specify what to order by</param>
    /// <returns>IFinalClause</returns>
    IFinalClause OrderBy(Fields field);

    /// <summary>
    ///     Specify the order by, ascending, this can be called mutiple times
    /// </summary>
    /// <param name="field">Field to specify what to order by</param>
    /// <returns>IFinalClause</returns>
    IFinalClause OrderByAscending(Fields field);

    /// <summary>
    ///     Specify the order by, descending, this can be called mutiple times
    /// </summary>
    /// <param name="field">Field to specify what to order by</param>
    /// <returns>IFinalClause</returns>
    IFinalClause OrderByDescending(Fields field);
}