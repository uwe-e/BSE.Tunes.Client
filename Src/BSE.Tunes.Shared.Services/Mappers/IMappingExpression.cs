using System.Linq.Expressions;

namespace BSE.Tunes.Shared.Services.Mappers
{
    /// <summary>
    /// Fluent interface for configuring mappings
    /// </summary>
    public interface IMappingExpression<TSource, TDestination>
    {
        /// <summary>
        /// Provides a custom mapping function
        /// </summary>
        IMappingExpression<TSource, TDestination> ConvertUsing(Func<TSource?, IMapper, TDestination?> mappingFunc);

        /// <summary>
        /// Maps a specific property
        /// </summary>
        IMappingExpression<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember,
            Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options);
    }

    public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        /// <summary>
        /// Maps from a source member
        /// </summary>
        void MapFrom(Func<TSource, TMember> sourceFunc);

        /// <summary>
        /// Maps from a source member with mapper access
        /// </summary>
        void MapFrom(Func<TSource, IMapper, TMember> sourceFunc);

        /// <summary>
        /// Ignores this member during mapping
        /// </summary>
        void Ignore();
    }

    internal class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
    {
        private readonly MappingConfiguration<TSource, TDestination> _config;

        public MappingExpression(MappingConfiguration<TSource, TDestination> config)
        {
            _config = config;
        }

        public IMappingExpression<TSource, TDestination> ConvertUsing(Func<TSource?, IMapper, TDestination?> mappingFunc)
        {
            _config.MappingFunc = mappingFunc;
            return this;
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember,
            Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> options)
        {
            var memberConfig = new MemberConfigurationExpression<TSource, TDestination, TMember>(destinationMember);
            options(memberConfig);

            if (memberConfig.SourceFunc != null)
            {
                var property = (destinationMember.Body as MemberExpression)?.Member as System.Reflection.PropertyInfo;
                if (property != null)
                {
                    _config.PropertyMappings.Add((src, dest, mapper) =>
                    {
                        var value = memberConfig.SourceFunc(src, mapper);
                        property.SetValue(dest, value);
                    });
                }
            }

            return this;
        }
    }

    internal class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>
    {
        private readonly Expression<Func<TDestination, TMember>> _destinationMember;
        
        public Func<TSource, IMapper, TMember>? SourceFunc { get; private set; }

        public MemberConfigurationExpression(Expression<Func<TDestination, TMember>> destinationMember)
        {
            _destinationMember = destinationMember;
        }

        public void MapFrom(Func<TSource, TMember> sourceFunc)
        {
            SourceFunc = (src, _) => sourceFunc(src);
        }

        public void MapFrom(Func<TSource, IMapper, TMember> sourceFunc)
        {
            SourceFunc = sourceFunc;
        }

        public void Ignore()
        {
            // No-op: property will not be mapped
        }
    }
}