namespace Microsoft.Extensions.DependencyInjection
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class LocalisationExtensions
    {
        public static IServiceCollection UseLocalisation(this IServiceCollection builder)
        {
            return builder.AddSingleton<ILocalisationService, LocalisationService>();
        }
    }
}