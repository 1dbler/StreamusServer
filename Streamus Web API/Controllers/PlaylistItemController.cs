﻿using System.Web.Http;
using Streamus_Web_API.Domain;
using Streamus_Web_API.Domain.Interfaces;
using Streamus_Web_API.Dto;
using log4net;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Streamus_Web_API.Controllers
{
  [RoutePrefix("PlaylistItem")]
  public class PlaylistItemController : StreamusController
  {
    private readonly IPlaylistManager _playlistManager;
    private readonly IPlaylistItemManager _playlistItemManager;

    public PlaylistItemController(ILog logger, ISession session, IManagerFactory managerFactory)
        : base(logger, session)
    {
      _playlistManager = managerFactory.GetPlaylistManager(session);
      _playlistItemManager = managerFactory.GetPlaylistItemManager(session);
    }

    [Route("")]
    [HttpPost]
    public PlaylistItemDto Create(PlaylistItemDto playlistItemDto)
    {
      PlaylistItemDto savedPlaylistItemDto;

      using (ITransaction transaction = Session.BeginTransaction())
      {
        Playlist playlist = _playlistManager.Get(playlistItemDto.PlaylistId);
        // TODO: Backwards compatibility for old type.
        VideoDto videoDto = playlistItemDto.Video ?? playlistItemDto.Song;

        PlaylistItem playlistItem = new PlaylistItem(playlistItemDto.Id, playlistItemDto.Cid, videoDto.Id, videoDto.Type, videoDto.Title, videoDto.Duration, videoDto.Author);
        playlistItemDto.SetPatchableProperties(playlistItem);

        playlist.AddItem(playlistItem);

        _playlistItemManager.Save(playlistItem);

        savedPlaylistItemDto = PlaylistItemDto.Create(playlistItem);

        transaction.Commit();
      }

      return savedPlaylistItemDto;
    }

    [Route("CreateMultiple")]
    [HttpPost]
    public IEnumerable<PlaylistItemDto> CreateMultiple(List<PlaylistItemDto> playlistItemDtos)
    {
      List<PlaylistItemDto> savedPlaylistItemDtos;

      int count = playlistItemDtos.Count;

      if (count > 1000)
        Session.SetBatchSize(count / 10);
      else if (count > 500)
        Session.SetBatchSize(count / 5);
      else if (count > 2)
        Session.SetBatchSize(count / 2);

      using (ITransaction transaction = Session.BeginTransaction())
      {
        List<PlaylistItem> savedPlaylistItems = new List<PlaylistItem>();

        //  Split items into their respective playlists and then save on each.
        foreach (var playlistGrouping in playlistItemDtos.GroupBy(pid => pid.PlaylistId))
        {
          List<PlaylistItemDto> groupedPlaylistItemDtos = playlistGrouping.ToList();
          Playlist playlist = _playlistManager.Get(playlistGrouping.Key);

          foreach (var playlistItemDto in groupedPlaylistItemDtos)
          {
            // TODO: Backwards compatibility for old type.
            VideoDto videoDto = playlistItemDto.Video ?? playlistItemDto.Song;

            PlaylistItem playlistItem = new PlaylistItem(playlistItemDto.Id, playlistItemDto.Cid, videoDto.Id, videoDto.Type, videoDto.Title, videoDto.Duration, videoDto.Author);
            playlistItemDto.SetPatchableProperties(playlistItem);

            playlist.AddItem(playlistItem);

            savedPlaylistItems.Add(playlistItem);
          }
        }

        _playlistItemManager.Save(savedPlaylistItems);

        savedPlaylistItemDtos = PlaylistItemDto.Create(savedPlaylistItems);

        transaction.Commit();
      }

      return savedPlaylistItemDtos;
    }

    [Route("{id:guid}")]
    [HttpPatch]
    public void Patch(Guid id, PlaylistItemDto playlistItemDto)
    {
      using (ITransaction transaction = Session.BeginTransaction())
      {
        PlaylistItem playlistItem = _playlistItemManager.Get(id);
        playlistItemDto.SetPatchableProperties(playlistItem);
        _playlistItemManager.Update(playlistItem);

        transaction.Commit();
      }
    }

    [Route("{id:guid}")]
    [HttpDelete]
    public void Delete(Guid id)
    {
      using (ITransaction transaction = Session.BeginTransaction())
      {
        _playlistItemManager.Delete(id);
        transaction.Commit();
      }
    }
  }
}
