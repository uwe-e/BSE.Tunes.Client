using BSEtunes.Contracts.DTOs.Albums;
using BSEtunes.Contracts.DTOs.Playlists;

namespace BSE.Tunes.Maui.Client.Services.Mappers.Profiles
{
    /// <summary>
    /// Mapping profile for DTO to domain model conversions
    /// </summary>
    public class DtoMappingProfile : MappingProfile
    {
        public DtoMappingProfile()
        {
            // Simple mapping with custom converter
            CreateMap<GenreDto, Genre>()
                .ConvertUsing((dto, mapper) => dto == null ? null : new Genre
                {
                    Id = dto.Id,
                    Name = dto.Name ?? string.Empty
                });

            CreateMap<ArtistDto, Artist>()
                .ConvertUsing((dto, mapper) => dto == null ? null : new Artist
                {
                    Id = dto.Id,
                    Name = dto.Name ?? string.Empty,
                    SortName = dto.SortName ?? string.Empty
                });

            // Complex mapping with nested mapper calls
            CreateMap<AlbumDto, Album>()
                .ConvertUsing((dto, mapper) =>
                {
                    if (dto == null) return null;
                    
                    if (dto.Artist == null)
                        throw new InvalidOperationException($"Album {dto.Id} has null Artist");

                    var alb = new Album
                    {
                        Id = dto.Id,
                        AlbumId = dto.AlbumId,
                        Title = dto.Title ?? string.Empty,
                        Year = dto.Year,
                        Thumbnail = dto.Thumbnail,
                        Cover = dto.Cover,
                        Genre = mapper.Map<Genre>(dto.Genre),
                        Artist = mapper.Map<Artist>(dto.Artist)!,
                        Tracks = mapper.MapCollection<Track>(dto.Tracks).ToArray()
                    };

                    return new Album
                    {
                        Id = dto.Id,
                        AlbumId = dto.AlbumId,
                        Title = dto.Title ?? string.Empty,
                        Year = dto.Year,
                        Thumbnail = dto.Thumbnail,
                        Cover = dto.Cover,
                        Genre = mapper.Map<Genre>(dto.Genre),
                        Artist = mapper.Map<Artist>(dto.Artist)!,
                        Tracks = mapper.MapCollection<Track>(dto.Tracks).ToArray()
                    };
                });

            CreateMap<TrackDto, Track>()
                .ConvertUsing((dto, mapper) =>
                {
                    if (dto == null) return null;

                    return new Track
                    {
                        Id = dto.Id,
                        TrackNumber = dto.TrackNumber,
                        Name = dto.Name ?? string.Empty,
                        Duration = dto.Duration,
                        Guid = dto.Guid,
                        Extension = dto.Extension ?? string.Empty,
                        Album = mapper.Map<Album>(dto.Album)!
                    };
                });

            CreateMap<PlaylistEntryDto, PlaylistEntry>()
                .ConvertUsing((dto, mapper) =>
                {
                    if (dto == null) return null;

                    if (dto.Track == null)
                        throw new InvalidOperationException($"PlaylistEntry {dto.Id} has null Track");

                    var track = mapper.Map<Track>(dto.Track);

                    return new PlaylistEntry
                    {
                        Id = dto.Id,
                        PlaylistId = dto.PlaylistId,
                        TrackId = dto.TrackId,
                        Name = track?.Name ?? string.Empty,
                        Artist = track?.Album?.Artist?.Name ?? string.Empty,
                        AlbumId = track?.Album?.AlbumId ?? Guid.Empty,
                        Duration = track?.Duration ?? TimeSpan.Zero,
                        Guid = dto.Guid,
                        Track = track!
                    };
                });
            
            CreateMap<PlaylistDto, Playlist>()
                .ConvertUsing((dto, mapper) =>
                {
                    if (dto == null) return null;
                    
                    return new Playlist
                    {
                        Id = dto.Id,
                        Guid = dto.Guid,
                        Name = dto.Name ?? string.Empty,
                        NumberEntries = dto.EntryCount.HasValue ? dto.EntryCount.Value : 0,
                        UserName = dto.Owner ?? string.Empty,
                        CoverAlbumIds = dto.CoverAlbumIds != null ? new List<string>(dto.CoverAlbumIds) : new List<string>(),
                        Entries = mapper.MapCollection<PlaylistEntry>(dto.Entries).ToList()
                    };
                });

        }
    }
}   