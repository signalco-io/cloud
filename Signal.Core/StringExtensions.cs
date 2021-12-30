﻿using System.IO;

namespace Signal.Core;

public static class StringExtensions
{
    public static string SanitizeFileName(this string fileName) => 
        string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
}