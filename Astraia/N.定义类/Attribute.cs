// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 21:08:27
// # Recently: 2026-08-14 21:58:27
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia;

[AttributeUsage(AttributeTargets.Field)]
public sealed class ExportAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public sealed class PrimaryAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
public sealed class SyncVarAttribute(string func = null) : Attribute
{
    public readonly string func = func;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ClientRpcAttribute(int pass = Pass.KCP) : Attribute
{
    public readonly int pass = pass;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class ServerRpcAttribute(int pass = Pass.KCP) : Attribute
{
    public readonly int pass = pass;
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class TargetRpcAttribute(int pass = Pass.KCP) : Attribute
{
    public readonly int pass = pass;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class UIPathAttribute(string asset) : Attribute
{
    public readonly string asset = asset;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class UIMaskAttribute(int layer, int group = 0) : Attribute
{
    public readonly int layer = layer;
    public readonly int group = group;
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class UIRectAttribute(int col, int row, float width, float height, float offset, bool rotation = true, bool selected = false) : Attribute
{
    public readonly int row = row;
    public readonly int col = col;
    public readonly float width = width;
    public readonly float height = height;
    public readonly float offset = offset;
    public readonly bool rotation = rotation;
    public readonly bool selected = selected;
}