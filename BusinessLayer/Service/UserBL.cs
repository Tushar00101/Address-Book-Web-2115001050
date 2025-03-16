using System;
using System.Collections.Generic;
using BusinessLayer.Interface;
using ModelLayer.Model;
using RepositoryLayer.Interface;
using RepositoryLayer.Entity;
using NLog;

namespace BusinessLayer.Service
{
    /// <summary>
    /// Handles business logic for user-related operations.
    /// </summary>
    public class UserBL : IUserBL
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IUserRL _userRL;

        /// <summary>
        /// Initializes a new instance of the UserBL class.
        /// </summary>
        /// <param name="userRL">Repository layer dependency for user operations.</param>
        public UserBL(IUserRL userRL)
        {
            _userRL = userRL;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDTO">User details for registration.</param>
        /// <returns>Registered user entity or null if already exists.</returns>
        public UserEntity RegisterBL(UserDTO userDTO)
        {
            logger.Info("Attempting user registration for email: {0}", userDTO.Email);
            var result = _userRL.RegisterRL(userDTO);
            if (result == null)
            {
                logger.Warn("User registration failed. Email already exists: {0}", userDTO.Email);
            }
            else
            {
                logger.Info("User registered successfully: {0}", userDTO.Email);
            }
            return result;
        }

        /// <summary>
        /// Handles user login by verifying credentials.
        /// </summary>
        /// <param name="loginDTO">Login credentials.</param>
        /// <returns>ResponseLoginDTO with user details if successful; otherwise, null.</returns>
        public ResponseLoginDTO LoginBL(LoginDTO loginDTO)
        {
            logger.Info("User login attempt for email: {0}", loginDTO.Email);

            var result = _userRL.LoginRL(loginDTO);
            if (result == null)
            {
                logger.Warn("Login failed. credentials not found: {0}", loginDTO.Email);
                return null;
            }
            
                logger.Info("User login successful: {0}", loginDTO.Email);
                return new ResponseLoginDTO { Email = result.Email, FirstName = result.FirstName, LastName = result.LastName ,Id=result.Id,Role=result.Role};
            
        }

        /// <summary>
        /// Handles forgot password requests.
        /// </summary>
        /// <param name="forgetDTO">User email for password recovery.</param>
        /// <returns>True if user exists; otherwise, false.</returns>
        public bool ForgetBL(ForgetDTO forgetDTO)
        {
            logger.Info("Password reset request for email: {0}", forgetDTO.Email);
            bool result = _userRL.ForgetRL(forgetDTO);
            if (!result)
            {
                logger.Warn("Password reset failed. User not found: {0}", forgetDTO.Email);
            }
            else
            {
                logger.Info("Password reset request successful for email: {0}", forgetDTO.Email);
            }
            return result;
        }

        public bool UpdateUserPassword(string email, ResetDTO resetDTO)
        {
            return _userRL.UpdateUserPassword(email,resetDTO);
        }

        public UserEntity GetUserProfile(int userId)
        {
            return _userRL.GetUserProfile(userId);
        }
    }
}