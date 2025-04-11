using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Manages HTTP response header creation and standardization across the application.
    /// Provides consistent header formatting for API responses.
    /// </summary>
    public interface IHeaderService
    {
        /// <summary>
        /// Creates a standardized header object for API responses.
        /// </summary>
        /// <param name="status">HTTP status code (defaults to 201 Created)</param>
        /// <param name="success">Indicates if the operation was successful (defaults to true)</param>
        /// <param name="responseDate">Optional custom response date (defaults to current time if null)</param>
        /// <returns>A Header object containing standardized response metadata</returns>
        /// <remarks>
        /// - Generates consistent header format
        /// - Includes status code and success indicator
        /// - Provides timestamp for response tracking
        /// - Supports custom status codes and response dates
        /// </remarks>
        public Header HeaderCreate(int status = (int)StatusEnum.SuccessfullyCreated, bool success = true, DateTime? responseDate = null);
    }
}
