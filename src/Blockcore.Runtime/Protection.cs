using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Blockcore.Runtime
{
    public class Protection
    {
        public Protection()
        {

        }

        public string Protect(string input)
        {
            // get the path to %LOCALAPPDATA%\myapp-keys
            string destFolder = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "blockcore-keys");

            // instantiate the data protection system at this folder
            var dataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(destFolder));

            var protector = dataProtectionProvider.CreateProtector("Blockcore");

            // protect the payload
            string protectedPayload = protector.Protect(input);
            Console.WriteLine($"Protect returned: {protectedPayload}");

            // unprotect the payload
            string unprotectedPayload = protector.Unprotect(protectedPayload);
            Console.WriteLine($"Unprotect returned: {unprotectedPayload}");

            return protectedPayload;
        }

        public string Unprotect(string input)
        {
            // get the path to %LOCALAPPDATA%\myapp-keys
            string destFolder = Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA"), "blockcore-keys");

            // instantiate the data protection system at this folder
            var dataProtectionProvider = DataProtectionProvider.Create(new DirectoryInfo(destFolder));

            var protector = dataProtectionProvider.CreateProtector("Blockcore");

            // unprotect the payload
            string unprotectedPayload = protector.Unprotect(input);
            Console.WriteLine($"Unprotect returned: {unprotectedPayload}");

            return unprotectedPayload;
        }

        public static void AddProtection(IServiceCollection services)
        {
            services.AddDataProtection().SetDefaultKeyLifetime(TimeSpan.FromDays(14));
        }
    }
}
