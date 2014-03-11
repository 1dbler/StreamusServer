﻿using FluentNHibernate.Mapping;
using Streamus_Web_API.Domain;

namespace Streamus_Web_API.Dao.Mappings
{
    public class VideoMapping : ClassMap<Video>
    {
        public VideoMapping()
        {
            Table("[Videos]");

            ReadOnly();

            //  Only update properties which have changed.
            DynamicUpdate();
            
            Id(e => e.Id).GeneratedBy.Assigned().Length(11);

            Map(e => e.Title).Not.Nullable();
            Map(e => e.Duration).Not.Nullable();
            Map(e => e.Author).Not.Nullable();
            Map(e => e.HighDefinition).Not.Nullable();
        }
    }
}