using Remora.Commands.Conditions;

namespace Spear.Conditions.Attributes;

/// <summary>
/// Indicates that a command requires the guild it's being invoked in to be registered first.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRegisteredGuildAttribute : ConditionAttribute {}
