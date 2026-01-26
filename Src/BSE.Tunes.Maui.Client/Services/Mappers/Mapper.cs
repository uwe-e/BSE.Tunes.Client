namespace BSE.Tunes.Maui.Client.Services.Mappers
{
    /// <summary>
    /// Lightweight object mapper implementation
    /// </summary>
    public class Mapper : IMapper
    {
        private readonly Dictionary<(Type Source, Type Destination), MappingConfiguration> _mappings = new();

        public Mapper(params MappingProfile[] profiles)
        {
            foreach (var profile in profiles)
            {
                RegisterProfile(profile);
            }
        }

        public void RegisterProfile(MappingProfile profile)
        {
            foreach (var config in profile.Configurations)
            {
                var key = (config.SourceType, config.DestinationType);
                _mappings[key] = config;
            }
        }

        public TDestination? Map<TDestination>(object? source)
        {
            if (source == null) return default;

            var sourceType = source.GetType();
            var destinationType = typeof(TDestination);
            var key = (sourceType, destinationType);

            if (_mappings.TryGetValue(key, out var config))
            {
                return (TDestination?)config.Map(source, this);
            }

            throw new InvalidOperationException(
                $"No mapping configured from {sourceType.Name} to {destinationType.Name}. " +
                $"Please register a mapping in a MappingProfile.");
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            var key = (typeof(TSource), typeof(TDestination));

            if (_mappings.TryGetValue(key, out var config))
            {
                return (TDestination)config.Map(source, this)!;
            }

            throw new InvalidOperationException(
                $"No mapping configured from {typeof(TSource).Name} to {typeof(TDestination).Name}");
        }

        public IEnumerable<TDestination> MapCollection<TDestination>(IEnumerable<object>? source)
        {
            if (source == null) return Enumerable.Empty<TDestination>();

            return source
                .Select(Map<TDestination>)
                .Where(item => item != null)
                .Cast<TDestination>();
        }
    }
}