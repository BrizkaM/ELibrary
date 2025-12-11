using ELibrary.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Api.Extensions
{
    /// <summary>
    /// Extension methods for converting ELibraryResult to ActionResult.
    /// Provides clean, reusable error handling for API controllers.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts ELibraryResult to ActionResult with proper HTTP status codes.
        /// </summary>
        public static ActionResult<T> ToActionResult<T>(
            this ELibraryResult<T> result,
            ControllerBase controller)
        {
            if (result.IsSuccess)
            {
                return controller.Ok(result.Value);
            }

            // Handle failure based on error code
            return result.ErrorCode switch
            {
                ErrorCodes.NotFound => controller.NotFound(new
                {
                    error = "Resource not found",
                    message = result.Error,
                    errorCode = result.ErrorCode
                }),

                ErrorCodes.ValidationError => controller.BadRequest(new
                {
                    error = "Validation error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                }),

                ErrorCodes.DuplicateIsbn => controller.Conflict(new
                {
                    error = "Duplicate resource",
                    message = result.Error,
                    errorCode = result.ErrorCode
                }),

                ErrorCodes.OutOfStock => controller.BadRequest(new
                {
                    error = "Out of stock",
                    message = result.Error,
                    errorCode = result.ErrorCode
                }),

                ErrorCodes.ConcurrencyConflict => controller.Conflict(new
                {
                    error = "Concurrency conflict",
                    message = result.Error,
                    errorCode = result.ErrorCode
                }),

                ErrorCodes.InvalidOperation => controller.StatusCode(500, new
                {
                    error = "Internal server error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                }),

                _ => controller.StatusCode(500, new
                {
                    error = "Unknown error",
                    message = result.Error,
                    errorCode = result.ErrorCode
                })
            };
        }

        /// <summary>
        /// Converts ELibraryResult to CreatedAtAction result for POST endpoints.
        /// </summary>
        public static ActionResult<T> ToCreatedResult<T>(
            this ELibraryResult<T> result,
            ControllerBase controller,
            string actionName,
            object? routeValues = null)
        {
            if (result.IsSuccess)
            {
                return controller.CreatedAtAction(actionName, routeValues, result.Value);
            }

            // For failures, use standard error handling
            return result.ToActionResult(controller);
        }

        /// <summary>
        /// Converts ELibraryResult to ActionResult with custom success status code.
        /// </summary>
        public static ActionResult<T> ToActionResult<T>(
            this ELibraryResult<T> result,
            ControllerBase controller,
            int successStatusCode)
        {
            if (result.IsSuccess)
            {
                return successStatusCode switch
                {
                    200 => controller.Ok(result.Value),
                    201 => new ObjectResult(result.Value) { StatusCode = 201 },
                    204 => controller.NoContent(),
                    _ => new ObjectResult(result.Value) { StatusCode = successStatusCode }
                };
            }

            return result.ToActionResult(controller);
        }
    }
}