﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ComputeSharp.BokehBlur.Processors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using ImageSharpRgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace Drizzle.ImageProcessing;

public static class ImageUtil
{
    //public static void GaussianBlurCompute(string src, string dest, float sigma)
    //{
    //    Configuration.Default.PreferContiguousImageBuffers = true;

    //    using Image<ImageSharpRgba32> image = Image.Load<ImageSharpRgba32>(src);
    //    image.Mutate(c => c.ApplyProcessor(new HlslGaussianBlurProcessor(sigma)));
    //    image.Save(dest);
    //}

    /// <summary>
    /// Gaussian image blur (CPU)
    /// </summary>
    /// <param name="src">Source image path</param>
    /// <param name="dest">Destination image path</param>
    /// <param name="sigma">Blur value</param>
    /// <param name="maxDimension">Fit the image within aspect ratio, 
    /// if width > height = (<paramref name="maxDimension"/>, newHeight) else (newWidth, <paramref name="maxDimension"/>)</param>
    public static void GaussianBlur(string src, string dest, float sigma, int maxDimension = 3840)
    {
        using var image = Image.Load(src);
        //Resize input for performance and memory
        if (image.Width > maxDimension || image.Height > maxDimension)
        {
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions()
                {
                    Size = new Size(maxDimension, maxDimension),
                    Mode = ResizeMode.Max
                });
            });
        }
        image.Mutate(x => x.GaussianBlur(sigma));
        image.Save(dest);
    }

    public static void GaussianBlur(Stream src, string dest, float sigma, int maxDimension = 3840)
    {
        using var image = Image.Load(src);
        //Resize input for performance and memory
        if (image.Width > maxDimension || image.Height > maxDimension)
        {
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions()
                {
                    Size = new Size(maxDimension, maxDimension),
                    Mode = ResizeMode.Max
                });
            });
        }
        image.Mutate(x => x.GaussianBlur(sigma));
        image.Save(dest);
    }

    public static Image<Rgba32> FloatArrayToImage(float[] floatArray, int width, int height)
    {
        var image = new Image<Rgba32>(width, height);
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int curPixel = j + i * width;
                image[j, i] = new Rgba32(floatArray[curPixel], floatArray[curPixel], floatArray[curPixel], 1);
            }
        }
        return image;
    }
}
