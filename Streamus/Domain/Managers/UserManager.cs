﻿using Streamus.Domain.Interfaces;
using System;
using log4net;

namespace Streamus.Domain.Managers
{
    /// <summary>
    ///     Provides a common spot for methods against Users which require transactions (Creating, Updating, Deleting)
    /// </summary>
    public class UserManager : AbstractManager, IUserManager
    {
        private IUserDao UserDao { get; set; }

        public UserManager(ILog logger, IUserDao userDao) 
            : base(logger) 
        {
            UserDao = userDao;
        }

        /// <summary>
        ///     Creates a new User and saves it to the DB. As a side effect, also creates a new, empty
        ///     Playlist and also saves it to the DB.
        /// </summary>
        /// <returns>The created user with a generated GUID</returns>
        public User CreateUser(string googlePlusId = "")
        {
            User user;

            try
            {
                user = new User
                    {
                        GooglePlusId = googlePlusId
                    };

                user.ValidateAndThrow();
                UserDao.Save(user);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }

            return user;
        }

        public void Save(User user)
        {
            try
            {
                user.ValidateAndThrow();
                UserDao.Save(user);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public void UpdateGooglePlusId(Guid userId, string googlePlusId)
        {
            try
            {
                UserDao.UpdateGooglePlusId(userId, googlePlusId);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
    }
}