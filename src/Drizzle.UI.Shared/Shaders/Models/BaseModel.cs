﻿using CommunityToolkit.Mvvm.ComponentModel;
using Drizzle.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Drizzle.UI.Shared.Shaders.Models;

public abstract class BaseModel : ObservableObject
{
    public ShaderTypes Type { get; }

    public float4 Mouse { get; set; } = float4.Zero;

    public float Brightness { get; set; } = 1f;

    public float Saturation { get; set; } = 1f;

    public float TimeMultiplier { get; set; } = 1f;

    protected BaseModel(ShaderTypes type)
    {
        this.Type = type;
    }
}
