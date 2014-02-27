﻿using NUnit.Framework;
using Streamus.Controllers;
using Streamus.Dao;
using Streamus.Domain;
using Streamus.Domain.Interfaces;
using Streamus.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Streamus.Tests.Controller_Tests
{
    [TestFixture]
    public class PlaylistItemControllerTest : AbstractTest
    {
        private PlaylistItemController PlaylistItemController;
        private IPlaylistDao PlaylistDao { get; set; }
        private Helpers Helpers;

        /// <summary>
        ///     This code is only ran once for the given TestFixture.
        /// </summary>
        [TestFixtureSetUp]
        public new void TestFixtureSetUp()
        {
            try
            {
                PlaylistItemController = new PlaylistItemController(Logger, DaoFactory, ManagerFactory);
                PlaylistDao = DaoFactory.GetPlaylistDao();

                Helpers = new Helpers(DaoFactory, ManagerFactory);
            }
            catch (TypeInitializationException exception)
            {
                throw exception.InnerException;
            }
        }

        [Test]
        public void CreatePlaylistItem_PlaylistItemDoesntExist_PlaylistItemCreated()
        {
            PlaylistItemDto playlistItemDto = Helpers.CreatePlaylistItemDto();

            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();
            var result = PlaylistItemController.Create(playlistItemDto);
            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

            var createdPlaylistItemDto = (PlaylistItemDto) result.Data;

            //  Make sure we actually get a PlaylistItem DTO back from the Controller.
            Assert.NotNull(createdPlaylistItemDto);

            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

            Playlist playlist = PlaylistDao.Get(createdPlaylistItemDto.PlaylistId);

            //  Make sure that the created playlistItem was cascade added to the Playlist
            Assert.That(playlist.Items.Count(i => i.Id == createdPlaylistItemDto.Id) == 1);

            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();
        }

        /// <summary>
        /// 50 PlaylistItems is the largest chunk expected to be saved in one burst because
        /// the YouTube API maxes out at 50 return items.
        /// </summary>
        [Test]
        public void Create50PlaylistItems_PlaylistEmpty_AllItemsCreated()
        {
            const int numItemsToCreate = 50;
            List<PlaylistItemDto> playlistItemDtos = Helpers.CreatePlaylistItemsDto(numItemsToCreate);

            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();
            var result = PlaylistItemController.CreateMultiple(playlistItemDtos);
            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

            var createdPlaylistItemDtos = (List<PlaylistItemDto>)result.Data;

            //  Make sure we actually get the list back from the Controller.
            Assert.NotNull(createdPlaylistItemDtos);
            Assert.That(createdPlaylistItemDtos.Count == numItemsToCreate);

            NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

            Playlist playlist = PlaylistDao.Get(playlistItemDtos.First().PlaylistId);

            //  Make sure that the created playlistItem was cascade added to the Playlist
            Assert.That(playlist.Items.Count == numItemsToCreate);

            NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();
        }

        /// <summary>
        /// A StackOverflowException will occur if mappings don't contain fetch="join." Ensure this rule is in place.
        /// </summary>
        [Test]
        public void CreatePlaylistItemsRepeatedly_PlaylistEmpty_NoStackOverflowException()
        {
            const int iterations = 2;
            const int numItemsToCreate = 2142;
            Guid playlistId = default(Guid);

            //  Starting at 1 because I want to use currentIteration to be used in math and makes more sense as 1.
            for (int currentIteration = 1; currentIteration <= iterations; currentIteration++)
            {
                List<PlaylistItemDto> playlistItemDtos = Helpers.CreatePlaylistItemsDto(numItemsToCreate, playlistId);

                NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

                var result = PlaylistItemController.CreateMultiple(playlistItemDtos);

                NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

                var createdPlaylistItemDtos = (List<PlaylistItemDto>)result.Data;

                //  Make sure we actually get the list back from the Controller.
                Assert.NotNull(createdPlaylistItemDtos);
                Assert.That(createdPlaylistItemDtos.Count == numItemsToCreate);

                NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

                Playlist playlist = PlaylistDao.Get(playlistItemDtos.First().PlaylistId);
                playlistId = playlist.Id;

                //  Make sure that the created playlistItem was cascade added to the Playlist
                Assert.That(playlist.Items.Count == numItemsToCreate * currentIteration);

                NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();
            }
        }

        [Test]
        public void CreateMaxPlaylistItems_PlaylistEmpty_DoesntTakeForFuckingEver()
        {
            const int iterations = 1;
            const int numItemsToCreate = 1;
            Guid playlistId = default(Guid);

            //  Starting at 1 because I want to use currentIteration to be used in math and makes more sense as 1.
            for (int currentIteration = 1; currentIteration <= iterations; currentIteration++)
            {
                List<PlaylistItemDto> playlistItemDtos = Helpers.CreatePlaylistItemsDto(numItemsToCreate, playlistId);

                NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

                var result = PlaylistItemController.CreateMultiple(playlistItemDtos);

                NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();

                var createdPlaylistItemDtos = (List<PlaylistItemDto>)result.Data;

                //  Make sure we actually get the list back from the Controller.
                Assert.NotNull(createdPlaylistItemDtos);
                Assert.That(createdPlaylistItemDtos.Count == numItemsToCreate);

                NHibernateSessionManager.Instance.OpenSessionAndBeginTransaction();

                Playlist playlist = PlaylistDao.Get(playlistItemDtos.First().PlaylistId);
                playlistId = playlist.Id;

                //  Make sure that the created playlistItem was cascade added to the Playlist
                Assert.That(playlist.Items.Count == numItemsToCreate * currentIteration);

                NHibernateSessionManager.Instance.CommitTransactionAndCloseSession();
            }
        }
    }
}
