using AliLib.Core;
using System;

namespace ProjectionSorcery;

public static class ModInfo
{
    public const string Name = "ProjectionSorcery";
    public const string Description = "Default.";
    public const string ModVersion = "2.0";

    public const string Author = "Allieum256";

    [ExportedString("manifest.json")]
    public const string QUICKHACK_MANIFEST =
$@"{{
    ""Name"": ""{Name}"",
    ""Description"": ""{Description}"",
    ""Author"": ""{Author}"",
    ""Version"": ""{ModVersion}"",
    ""GameVersion"": ""1.0.0.0"",
    ""Thumbnail"": """"
}}";
}

