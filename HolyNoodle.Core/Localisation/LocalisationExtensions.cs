namespace Microsoft.Extensions.DependencyInjection
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class LocalisationExtensions
    {
        public static IServiceCollection AddHolyNoodleLocalisation(this IServiceCollection services)
        {
            return services.AddSingleton<ILocalisationService, LocalisationService>();
        }
    }
}