using Remora.Results;
using Spear.Models;

namespace Spear.Results;

public record SpearPermissionDeniedError(string Message, params Permission[] Permissions) : ResultError(Message);
