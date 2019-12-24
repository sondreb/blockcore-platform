namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void AddJsonFileFromArgument(this IConfigurationBuilder builder, string[] args)
        {
            // Right now the first parameter must be config if adding a config override file.
            if (args.Length > 0 && args[0].StartsWith("config"))
            {
                builder.AddJsonFile(args[0].Substring(7));
            }
        }
    }
}
