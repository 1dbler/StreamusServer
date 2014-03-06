using System;
using FluentNHibernate.Mapping;
using Streamus.Domain;

namespace Streamus.Dao.Mappings
{
    public class PlaylistMapping : ClassMap<Playlist>
    {
        public PlaylistMapping()
        {
            Table("[Playlists]");
            Id(e => e.Id).GeneratedBy.GuidComb().UnsavedValue(Guid.Empty);

            Map(e => e.Title).Not.Nullable();
            Map(e => e.Sequence).Not.Nullable();

            //  Only update properties which have changed.
            DynamicUpdate();

            HasMany(p => p.Items).Inverse().Fetch.Join().Cascade.AllDeleteOrphan().KeyColumn("PlaylistId");

            References(p => p.User).Column("UserId");
        }
    }
}