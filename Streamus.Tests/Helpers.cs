﻿using System;
using System.Collections.Generic;
using System.Linq;
using Streamus.Dao;
using Streamus.Domain;
using Streamus.Domain.Interfaces;
using Streamus.Domain.Managers;
using Streamus.Dto;

namespace Streamus.Tests
{
    /// <summary>
    ///     Stores common methods used by tests. Just useful for keeping things DRY between test cases.
    /// </summary>
    public class Helpers
    {
        private readonly IUserManager UserManager;
        private readonly IPlaylistItemManager PlaylistItemManager;

        public Helpers(IDaoFactory daoFactory, IManagerFactory managerFactory)
        {
            IUserDao userDao = daoFactory.GetUserDao();
            UserManager = managerFactory.GetUserManager(userDao);

            IPlaylistItemDao playlistItemDao = daoFactory.GetPlaylistItemDao();
            IVideoDao videoDao = daoFactory.GetVideoDao();
            PlaylistItemManager = managerFactory.GetPlaylistItemManager(playlistItemDao, videoDao);
        }

        /// <summary>
        ///     Creates a new Video and PlaylistItem, puts item in the database and then returns
        ///     the item. Just a nice utility method to keep things DRY.
        /// </summary>
        public PlaylistItem CreateItemInPlaylist(Playlist playlist)
        {
            Video videoNotInDatabase = CreateUnsavedVideoWithId();

            //  Create a new PlaylistItem and write it to the database.
            string title = videoNotInDatabase.Title;
            var playlistItem = new PlaylistItem(title, videoNotInDatabase);

            playlist.AddItem(playlistItem);

            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();
            PlaylistItemManager.Save(playlistItem);
            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

            return playlistItem;
        }

        /// <summary>
        ///     Creates a new Video with a random Id, or a given Id if specified, saves it to the database and returns it.
        /// </summary>
        public Video CreateUnsavedVideoWithId(string idOverride = "", string titleOverride = "")
        {
            //  Create a random video ID to ensure the Video doesn't exist in the database currently.
            string randomVideoId = idOverride == string.Empty ? Guid.NewGuid().ToString().Substring(0, 11) : idOverride;
            string title = titleOverride == string.Empty ? string.Format("Video {0}", randomVideoId) : titleOverride;
            var video = new Video(randomVideoId, title, 999, "Author");

            return video;
        }

        public User CreateUser()
        {
            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

            User user = UserManager.CreateUser();

            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

            return user;
        }

        /// <summary>
        ///     Generate a PlaylistDto which has the User as its parent.
        /// </summary>
        /// <returns></returns>
        public PlaylistDto CreatePlaylistDto(Guid userIdOverride)
        {
            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

            var playlistDto = new PlaylistDto
                {
                    UserId = userIdOverride
                };

            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

            return playlistDto;
        }

        /// <summary>
        ///     Create a new Playlist, save it to the DB, then generate a PlaylistItemDto
        ///     which has the Playlist as its parent.
        /// </summary>
        public PlaylistItemDto CreatePlaylistItemDto()
        {
            User user = CreateUser();

            Guid playlistId = user.Playlists.First().Id;

            Video video = CreateUnsavedVideoWithId();
            VideoDto videoDto = VideoDto.Create(video);

            var playlistItemDto = new PlaylistItemDto
                {
                    PlaylistId = playlistId,
                    Video = videoDto
                };

            return playlistItemDto;
        }

        /// <summary>
        ///     Create a new Playlist, save it to the DB, then generate N PlaylistItemDtos
        ///     which have the Playlist as their parent.
        /// </summary>
        public List<PlaylistItemDto> CreatePlaylistItemsDto(int itemsToCreate, Guid playlistId = default(Guid))
        {
            if (playlistId == default(Guid))
            {
                User user = CreateUser();
                playlistId = user.Playlists.First().Id;
            }

            Video video = CreateUnsavedVideoWithId();
            VideoDto videoDto = VideoDto.Create(video);

            List<PlaylistItemDto> playlistItemDtos = new List<PlaylistItemDto>();

            for (int i = 0; i < itemsToCreate; i++)
            {
                var playlistItemDto = new PlaylistItemDto
                {
                    PlaylistId = playlistId,
                    Video = videoDto
                };

                playlistItemDtos.Add(playlistItemDto);
            }

            return playlistItemDtos;
        }
    }
}
