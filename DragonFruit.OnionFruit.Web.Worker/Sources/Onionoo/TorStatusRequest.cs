// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using DragonFruit.Data;
using DragonFruit.Data.Requests;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public partial class TorStatusRequest : ApiRequest
{
    public override string RequestPath => "https://onionoo.torproject.org/details";

    /// <summary>
    /// The date the previous request was made.
    /// If set, data will only be returned if a newer version is available
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }

    /// <summary>
    /// Query to filter nodes by
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_search
    /// </remarks>
    [RequestParameter(ParameterType.Query, "search")]
    public string Query { get; set; }

    /// <summary>
    /// Filter nodes by hostname
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_host_name
    /// </remarks>
    [RequestParameter(ParameterType.Query, "host_name")]
    public string Hostname { get; set; }

    /// <summary>
    /// When set, filters nodes by the currently recommended version (as defined by Tor)
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_recommended_version
    /// </remarks>
    [RequestParameter(ParameterType.Query, "recommended_version")]
    public bool? FilterRecommendedVersion { get; set; }

    /// <summary>
    /// Optional country code to filter nodes by
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_country
    /// </remarks>
    [RequestParameter(ParameterType.Query, "country")]
    public string CountryCode { get; set; }

    /// <summary>
    /// Optional operating system filter
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_os
    /// </remarks>
    [RequestParameter(ParameterType.Query, "os")]
    public string OperatingSystem { get; set; }

    /// <summary>
    /// When set, filters out nodes by their current uptime status
    /// </summary>
    [RequestParameter(ParameterType.Query, "running")]
    public bool? Running { get; set; }

    /// <summary>
    /// Optional offset
    /// </summary>
    [RequestParameter(ParameterType.Query, "offset")]
    public int? Offset { get; set; }

    /// <summary>
    /// Optional limit
    /// </summary>
    [RequestParameter(ParameterType.Query, "limit")]
    public int? Limit { get; set; }

    /// <summary>
    /// Defines what type of server should be returned
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_type
    /// </remarks>
    [EnumOptions(EnumOption.StringLower)]
    [RequestParameter(ParameterType.Query, "type")]
    public virtual TorNodeType? Type { get; set; }

    /// <summary>
    /// Reorder the response data (defaults to ascending order).
    /// To order by descending order, prefix the field with a minus symbol (-)
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_order
    /// </remarks>
    [EnumerableOptions(EnumerableOption.Concatenated)]
    [RequestParameter(ParameterType.Query, "order")]
    public IEnumerable<string> Order { get; set; }

    /// <summary>
    /// The specific fields to return. Defaults to return everything
    /// </summary>
    /// <remarks>
    /// https://metrics.torproject.org/onionoo.html#parameters_fields
    /// </remarks>
    [EnumerableOptions(EnumerableOption.Concatenated)]
    [RequestParameter(ParameterType.Query, "fields")]
    public IEnumerable<string> ResponseFields { get; set; }

    [RequestParameter(ParameterType.Header, "If-Modified-Since")]
    protected string LastModifiedString => LastModified?.ToString("R");
}