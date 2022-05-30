using Remora.Results;

namespace Spear.Extensions;

public static class ResultExtensions {
    public static IResultError? GetFirstInnerErrorOfNotType<T>(this IResult result) {
        var innerResult = result;

        while(innerResult is not null) {
            if(innerResult.Error?.GetType() != typeof(T)) {
                return innerResult.Error;
            }

            innerResult = innerResult.Inner;
        }

        return null;
    }
}
