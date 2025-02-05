﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.Models.Weather;

public class ForecastAirQuality
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Name { get; set; }
    public string TimeZone { get; set; }
    public DateTime FetchTime { get; set; }
    public ForecastAirQualityUnits Units { get; set; }
    public IReadOnlyList<DailyAirQuality> Daily { get; set; }
}