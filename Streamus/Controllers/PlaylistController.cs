﻿using System;
using System.Reflection;
using System.Web.Mvc;
using Streamus.Dao;
using Streamus.Domain;
using Streamus.Domain.Interfaces;
using Streamus.Domain.Managers;
using Streamus.Dto;
using log4net;

namespace Streamus.Controllers
{
    [SessionManagement]
    public class PlaylistController : Controller
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly PlaylistManager PlaylistManager = new PlaylistManager();

        private readonly IPlaylistDao PlaylistDao;
        private readonly IUserDao UserDao;
        private readonly IShareCodeDao ShareCodeDao;

        public PlaylistController()
        {
            try
            {
                PlaylistDao = new PlaylistDao();
                UserDao = new UserDao();
                ShareCodeDao = new ShareCodeDao();
            }
            catch (TypeInitializationException exception)
            {
                Logger.Error(exception.InnerException);
                throw exception.InnerException;
            }
        }

        [HttpPost]
        public ActionResult Create(PlaylistDto playlistDto)
        {
            Playlist playlist = Playlist.Create(playlistDto);
            playlist.User.AddPlaylist(playlist);

            //  Make sure the playlist has been setup properly before it is cascade-saved through the User.
            playlist.ValidateAndThrow();

            PlaylistManager.Save(playlist);

            PlaylistDto savedPlaylistDto = PlaylistDto.Create(playlist);

            return new JsonServiceStackResult(savedPlaylistDto);
        }

        [HttpPut]
        public ActionResult Update(PlaylistDto playlistDto)
        {
            Playlist playlist = Playlist.Create(playlistDto);
            PlaylistManager.Update(playlist);

            PlaylistDto updatedPlaylistDto = PlaylistDto.Create(playlist);
            return new JsonServiceStackResult(updatedPlaylistDto);
        }

        [HttpGet]
        public ActionResult Get(Guid id)
        {
            Playlist playlist = PlaylistDao.Get(id);
            PlaylistDto playlistDto = PlaylistDto.Create(playlist);

            return new JsonServiceStackResult(playlistDto);
        }

        [HttpDelete]
        public JsonResult Delete(Guid id)
        {
            PlaylistManager.Delete(id);

            return Json(new
                {
                    success = true
                });
        }

        [HttpPost]
        public JsonResult UpdateTitle(Guid playlistId, string title)
        {
            PlaylistManager.UpdateTitle(playlistId, title);

            return Json(new
                {
                    success = true
                });
        }

        /// <summary>
        ///     Retrieves a ShareCode relating to a Playlist, create a copy of the Playlist referenced by the ShareCode,
        ///     and return the copied Playlist.
        /// </summary>
        [HttpGet]
        public JsonResult CreateCopyByShareCode(string shareCodeShortId, string urlFriendlyEntityTitle, Guid userId)
        {
            ShareCode shareCode = ShareCodeDao.GetByShortIdAndEntityTitle(shareCodeShortId, urlFriendlyEntityTitle);

            if (shareCode == null)
            {
                throw new ApplicationException("Unable to locate shareCode in database.");
            }

            if (shareCode.EntityType != ShareableEntityType.Playlist)
            {
                throw new ApplicationException("Expected shareCode to have entityType of Playlist");
            }

            //  Never return the sharecode's playlist reference. Make a copy of it to give out so people can't modify the original.
            Playlist playlistToCopy = PlaylistDao.Get(shareCode.EntityId);

            User user = UserDao.Get(userId);

            var playlistCopy = new Playlist(playlistToCopy);
            user.AddPlaylist(playlistCopy);

            PlaylistManager.Save(playlistCopy);

            PlaylistDto playlistDto = PlaylistDto.Create(playlistCopy);
            return new JsonServiceStackResult(playlistDto);
        }
    }
}
