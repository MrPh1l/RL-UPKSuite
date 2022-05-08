﻿using Core.Types;

namespace Core.Classes.Core.Properties;

/// <summary>
///     Property for a TMap value
/// </summary>
[NativeOnlyClass("Core", "MapProperty", "Property")]
public class UMapProperty : UProperty
{
    /// <inheritdoc />
    public UMapProperty(FName name, UClass? @class, UObject? outer, UnrealPackage ownerPackage, UObject? objectArchetype = null) : base(name, @class,
        outer,
        ownerPackage, objectArchetype)
    {
    }
}