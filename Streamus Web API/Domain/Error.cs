﻿using System;
using FluentValidation;
using Streamus_Web_API.Domain.Validators;

namespace Streamus_Web_API.Domain
{
    //  TODO: Consider renaming this "ClientError" instead of "Error" to be more explicit and not conflict with Error namespace.
    public class Error : AbstractDomainEntity<Guid>
    {
        public virtual string Message { get; set; }
        public virtual int LineNumber { get; set; }
        public virtual string Url { get; set; }
        public virtual string ClientVersion { get; set; }
        public virtual DateTime TimeOccurred { get; set; }
        public virtual string OperatingSystem { get; set; }
        public virtual string Architecture { get; set; }

        public Error()
        {
            Message = string.Empty;
            LineNumber = -1;
            Url = string.Empty;
            ClientVersion = string.Empty;
            TimeOccurred = DateTime.Now;
            OperatingSystem = string.Empty;
            Architecture = string.Empty;
        }

        public Error(string architecture, string clientVersion, int lineNumber, string message, string operatingSystem, string url)
            : this()
        {
            Architecture = architecture;
            ClientVersion = clientVersion;
            LineNumber = lineNumber;
            Message = message;
            OperatingSystem = operatingSystem;
            Url = url;

            if (Message.Length > 255)
            {
                //  When receiving an error message from the client -- ensure it is a maximum of 255 characters before saving.
                Message = string.Format("{0}...", Message.Substring(0, 252));
            }
        }

        public virtual void ValidateAndThrow()
        {
            var validator = new ErrorValidator();
            validator.ValidateAndThrow(this);
        }
    }
}