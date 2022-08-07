﻿using CommandLine;

internal class BatchProcessOptions
{
    [Option('f', "folder", Required = true, HelpText = "Path to the folder containing the packages")]
    public string InputDirectory { get; set; } = string.Empty;

    [Option('f', "class_folder", Required = true, HelpText = "Path to the folder containing the classp ackages (core,engine, etc)")]
    public string ClassPackagesDirectory { get; set; } = string.Empty;

    [Option('o', "out", Required = true, HelpText = "Path to the folder where to place the exported packages")]
    public string OutputDirectory { get; set; } = string.Empty;

    [Option('g', "glob", Default = "*.upk", Required = false, HelpText = "Glob pattern for filtering packages")]
    public string GlobPattern { get; set; } = string.Empty;

    [Option('k', "keys", Default = "keys.txt", Required = false, HelpText = "Path to key list")]
    public string KeysPath { get; set; } = string.Empty;
}