﻿using Drizzle.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Common.Constants;

public static class UserSettingsConstants
{
    public const string Performance = "Performance";

    public const string WeatherUnit = "WeatherUnit";

    public const string Theme = "AppTheme";

    public const string AutoHideScreensaverMenu = "AutoHideScreensaverMenu";

    public const string IncludeUserImagesInShuffle = "IncludeUserImagesInShuffle";

    public const string OpenWeatherMapKey = "OpenWeatherMapKey";

    public const string PinnedLocations = "PinnedLocations";

    public const string SelectedLocation = "SelectedLocation";

    public const string MaxPinnedLocations = "MaxPinnedLocations";

    public const string CacheWeather = "CacheWeather";

    public const string BackgroundBrightness = "BackgroundBrightness";

    public static IReadOnlyDictionary<string, object> Defaults { get; } = new Dictionary<string, object>()
    {
        { Performance, AppPerformance.performance },
        { WeatherUnit, UserWeatherUnits.metric },
        { Theme, AppTheme.dark },
        { AutoHideScreensaverMenu, false },
        { IncludeUserImagesInShuffle, false },
        { OpenWeatherMapKey, string.Empty },
        { PinnedLocations, Array.Empty<LocationModel>() },
        { SelectedLocation, null },
        { MaxPinnedLocations, 3 },
        { CacheWeather, true },
        { BackgroundBrightness, 1f },
    };
}
