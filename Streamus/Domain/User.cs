﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentValidation;
using Streamus.Domain.Validators;
using Streamus.Dto;

namespace Streamus.Domain
{
    public class User : AbstractDomainEntity<Guid>
    {
        public string Name { get; set; }
        public string GooglePlusId { get; set; }
        //  Use interfaces so NHibernate can inject with its own collection implementation.
        public ICollection<Playlist> Playlists { get; set; }

        public User()
        {
            Name = string.Empty;
            GooglePlusId = string.Empty;
            Playlists = new List<Playlist>();

            //  A user should always have at least one Playlist.
            CreateAndAddPlaylist();
        }

        public static User Create(UserDto userDto)
        {
            User user = Mapper.Map<UserDto, User>(userDto);
            return user;
        }

        public Playlist CreateAndAddPlaylist()
        {
            string title = string.Format("Playlist {0:D4}", Playlists.Count);
            var playlist = new Playlist(title)
                {
                    User = this
                };
            Playlists.Add(playlist);

            return playlist;
        }

        public void RemovePlaylist(Playlist playlist)
        {
            Playlists.Remove(playlist);
        }

        public void ValidateAndThrow()
        {
            var validator = new UserValidator();
            validator.ValidateAndThrow(this);
        }

        public virtual void AddPlaylist(Playlist playlist)
        {
            //  Client might not set the sequence number.
            if (playlist.Sequence < 0)
            {
                if (Playlists.Any())
                {
                    playlist.Sequence = Playlists.OrderBy(i => i.Sequence).Last().Sequence + 10000;
                }
                else
                {
                    playlist.Sequence = 10000;
                }
            }

            playlist.User = this;
            Playlists.Add(playlist);
        }
    }
}