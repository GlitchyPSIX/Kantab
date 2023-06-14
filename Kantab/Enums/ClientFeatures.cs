using System;

namespace Kantab.Enums;

[Flags]
public enum ClientFeatures {
    ABSOLUTE_POSITION = 1,
    EXTENDED_DATA = 2,
    RELAY_AUTHORITY = 4
}