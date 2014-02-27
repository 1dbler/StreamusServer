﻿using Streamus.Dao;
using Streamus.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace Streamus.Domain.Managers
{
    public class VideoManager : AbstractManager, IVideoManager
    {
        private IVideoDao VideoDao { get; set; }

        public VideoManager(ILog logger, IVideoDao videoDao)
            : base(logger) 
        {
            VideoDao = videoDao;
        }

        public void Save(Video video)
        {
            try
            {
                DoSave(video);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public void Save(IEnumerable<Video> videos)
        {
            try
            {
                List<Video> videosList = videos.ToList();

                if (videosList.Count > 1000)
                {
                    NHibernateSessionManager.Instance.SessionFactory.GetCurrentSession().SetBatchSize(videosList.Count / 10);
                }
                else
                {
                    NHibernateSessionManager.Instance.SessionFactory.GetCurrentSession().SetBatchSize(videosList.Count / 5);
                }

                videosList.ForEach(DoSave);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        private void DoSave(Video video)
        {
            video.ValidateAndThrow();

            //  Merge instead of SaveOrUpdate because Video's ID is assigned, but the same Video
            //  entity can be referenced by many different Playlists. As such, it is common to have the entity
            //  loaded into the cache.
            VideoDao.Merge(video);
        }
    }
}