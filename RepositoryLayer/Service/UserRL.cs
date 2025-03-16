using System;
using System.Linq;
using ModelLayer.Model;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using NLog;
using Security;

namespace RepositoryLayer.Service
{
    /// <summary>
    /// Handles user-related database operations such as registration, login, and password recovery.
    /// </summary>
    public class UserRL : IUserRL
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly UserContext _context;
        RedisCache _cacheService;

        /// <summary>
        /// Initializes a new instance of the UserRL class.
        /// </summary>
        /// <param name="context">Database context for user entity.</param>
        public UserRL(UserContext context, RedisCache cacheService)
        {
            _context = context;
            //_cacheService = cacheService;
        }

        /// <summary>
        /// Registers a new user if the email is not already in use.
        /// </summary>
        /// <param name="userDTO">Data transfer object containing user details.</param>
        /// <returns>Registered user entity if successful; otherwise, null.</returns>
        public UserEntity RegisterRL(UserDTO userDTO)
        {
            logger.Info("Attempting to register new user with email: {0}", userDTO.Email);

            var existingUser = _context.Users.FirstOrDefault(e => e.Email == userDTO.Email);
            if (existingUser == null)
            {
                string userRole = string.IsNullOrEmpty(userDTO.Role) ? "User" : userDTO.Role; // Set default role if null

                var user = new UserEntity
                {
                    Email = userDTO.Email,
                    Password = PasswordHelper.HashPassword(userDTO.Password), // Ensure password is hashed
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Role = userRole
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                logger.Info("User registered successfully with email: {0} and role: {1}", userDTO.Email, userRole);
                return user;
            }

            logger.Warn("Registration failed: Email {0} is already in use.", userDTO.Email);
            return null;
        }


        /// <summary>
        /// Authenticates a user based on email and password.
        /// </summary>
        /// <param name="loginDTO">Data transfer object containing login credentials.</param>
        /// <returns>User entity if credentials are valid; otherwise, null.</returns>
        public UserEntity LoginRL(LoginDTO loginDTO)
        {
            logger.Info("User login attempt with email: {0}", loginDTO.Email);

            var user = _context.Users.FirstOrDefault(e => e.Email == loginDTO.Email);
            if (user == null)
            {
                logger.Warn("Login failed: No user found with email {0}", loginDTO.Email);
                return null;
            }

            if(PasswordHelper.VerifyPassword(loginDTO.Password,user.Password))
            {
            logger.Info("Login successful for email: {0}", loginDTO.Email);
            return user;
            }
            logger.Warn("Login failed: credentials not matched", loginDTO.Email);
            return null;

        }

        /// <summary>
        /// Checks if a user exists with the provided email for password reset purposes.
        /// </summary>
        /// <param name="forgetDTO">Data transfer object containing the email.</param>
        /// <returns>True if user exists; otherwise, false.</returns>
        public bool ForgetRL(ForgetDTO forgetDTO)
        {
            logger.Info("Password reset request received for email: {0}", forgetDTO.Email);

            var userExists = _context.Users.Any(e => e.Email == forgetDTO.Email);
            if (!userExists)
            {
                logger.Warn("Password reset failed: No user found with email {0}", forgetDTO.Email);
                return false;
            }

            logger.Info("Password reset request successful for email: {0}", forgetDTO.Email);
            return true;
        }

        public bool UpdateUserPassword(string email,ResetDTO resetDTO)
        {
            var user=_context.Users.FirstOrDefault(e=>e.Email == email);
            if(user == null) return false;

            user.Password=PasswordHelper.HashPassword(resetDTO.Password);
            _context.Users.Update(user);
            _context.SaveChanges();
            return true;
        }

        public UserEntity GetUserProfile(int userId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userId);
        }
    }
}
