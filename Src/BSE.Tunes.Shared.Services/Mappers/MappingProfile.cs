namespace BSE.Tunes.Shared.Services.Mappers
{
    /// <summary>
    /// Base class for defining mapping configurations
    /// </summary>
    public abstract class MappingProfile
    {
        internal List<MappingConfiguration> Configurations { get; } = new();

        /// <summary>
        /// Creates a mapping from TSource to TDestination
        /// </summary>
        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            var config = new MappingConfiguration<TSource, TDestination>();
            Configurations.Add(config);
            return new MappingExpression<TSource, TDestination>(config);
        }
    }

    internal abstract class MappingConfiguration
    {
        public abstract Type SourceType { get; }
        public abstract Type DestinationType { get; }
        public abstract object? Map(object? source, IMapper mapper);
    }

    internal class MappingConfiguration<TSource, TDestination> : MappingConfiguration
    {
        public override Type SourceType => typeof(TSource);
        public override Type DestinationType => typeof(TDestination);
        
        public Func<TSource?, IMapper, TDestination?>? MappingFunc { get; set; }
        public List<Action<TSource, TDestination, IMapper>> PropertyMappings { get; } = new();

        public override object? Map(object? source, IMapper mapper)
        {
            if (source == null) return default(TDestination);

            var typedSource = (TSource)source;
            
            if (MappingFunc != null)
            {
                return MappingFunc(typedSource, mapper);
            }

            // Create destination instance
            var destination = Activator.CreateInstance<TDestination>();
            
            // Apply property mappings
            foreach (var propertyMapping in PropertyMappings)
            {
                propertyMapping(typedSource, destination, mapper);
            }

            return destination;
        }
    }
}