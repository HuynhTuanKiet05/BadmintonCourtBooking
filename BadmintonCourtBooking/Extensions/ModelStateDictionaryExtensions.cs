using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BadmintonCourtBooking.Extensions;

public static class ModelStateDictionaryExtensions
{
    public static string GetFirstErrorMessage(this ModelStateDictionary modelState, string fallbackMessage)
    {
        return modelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
            ?? fallbackMessage;
    }
}
