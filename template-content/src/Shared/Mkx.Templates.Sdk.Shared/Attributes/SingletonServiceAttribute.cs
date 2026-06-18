using System;

namespace Mkx.Templates.Sdk.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonServiceAttribute : Attribute;
