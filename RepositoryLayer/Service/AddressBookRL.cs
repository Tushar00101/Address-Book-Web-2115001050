using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ModelLayer.Model;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using NLog;

namespace RepositoryLayer.Service
{
    public class AddressBookRL : IAddressBookRL
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly UserContext _context;
        private readonly RedisCache _cacheService;

        public AddressBookRL(UserContext context, RedisCache cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Adds a new address book entry for a user.
        /// </summary>
        public ResponseModel<string> AddEntry(AddressBookDTO requestModel, int userId)
        {
            logger.Info($"Adding new address book entry for UserId: {userId}");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                logger.Warn($"User with ID {userId} not found.");
                return new ResponseModel<string> { Success = false, Message = "User not found" };
            }

            var entry = new AddressBookEntity
            {
                Name = requestModel.Name,
                Email = requestModel.Email,
                PhoneNumber = requestModel.PhoneNumber,
                Address = requestModel.Address,
                UserId = userId
            };

            _context.AddressBook.Add(entry);
            _context.SaveChanges();

            logger.Info("Address book entry added successfully.");
            return new ResponseModel<string> { Success = true, Message = "Entry added successfully" };
        }

        /// <summary>
        /// Retrieves an address book entry by ID.
        /// </summary>
        public AddressBookEntity GetEntryById(int id)
        {
            logger.Info($"Fetching address book entry with ID: {id}");
            var entry = _context.AddressBook.FirstOrDefault(e => e.Id == id);

            if (entry == null)
            {
                logger.Warn($"Entry with ID {id} not found.");
                return null;
            }

            logger.Info($"Entry with ID {id} retrieved successfully.");
            return entry;
        }

        /// <summary>
        /// Retrieves all address book entries for a user.
        /// </summary>
        public List<ResponseContactListDTO> GetAllEntries(int userId)
        {
            string cacheKey = $"AddressBook_{userId}";

            var cachedData = _cacheService.GetData(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                var entries = JsonSerializer.Deserialize<List<ResponseContactListDTO>>(cachedData);
                if (entries != null && entries.Any())
                {
                    logger.Info($"Data retrieved from Redis for UserId: {userId}");
                    return entries;
                }
            }

            logger.Info($"Fetching address book entries from DB for UserId: {userId}");
            var result = _context.AddressBook
                .Where(e => e.UserId == userId)
                .Select(e => new ResponseContactListDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    PhoneNumber = e.PhoneNumber,
                    Email = e.Email,
                    Address = e.Address
                })
                .ToList(); ;

            if (!result.Any())
            {
                logger.Warn($"No address book entries found for UserId: {userId}");
            }
            else
            {
                logger.Info($"Retrieved {result.Count} entries for UserId: {userId}");
                _cacheService.SaveCache(cacheKey, result);
            }

            return result;
        }

        /// <summary>
        /// Updates an existing address book entry.
        /// </summary>
        public AddressBookEntity UpdateEntry(UpdateRequest requestModel, int userId)
        {
            logger.Info($"Updating address book entry ID: {requestModel.Id}, UserId: {userId}");

            var entry = _context.AddressBook.FirstOrDefault(e => e.Id == requestModel.Id && e.UserId == userId);
            if (entry == null)
            {
                logger.Warn($"Entry with ID {requestModel.Id} not found or doesn't belong to UserId {userId}.");
                return null;
            }

            entry.Name = requestModel.Name;
            entry.PhoneNumber = requestModel.PhoneNumber;
            entry.Address = requestModel.Address;
            _context.SaveChanges();

            logger.Info($"Entry with ID {requestModel.Id} updated successfully.");
            return entry;
        }
        /// <summary>
        /// Deletes an address book entry.
        /// </summary>
        public AddressBookEntity DeleteEntry(int id, int userId)
        {
            logger.Info($"Attempting to delete address book entry ID: {id}, UserId: {userId}");

            var entry = _context.AddressBook.FirstOrDefault(e => e.Id == id && e.UserId == userId);
            if (entry == null)
            {
                logger.Warn($"Entry with ID {id} not found or doesn't belong to UserId {userId}.");
                return null;
            }

            _context.AddressBook.Remove(entry);
            _context.SaveChanges();

            logger.Info($"Entry with ID {id} deleted successfully.");

            string cacheKey = $"AddressBook_{userId}";
            var cachedData = _cacheService.GetData(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                var entries = JsonSerializer.Deserialize<List<ResponseContactListDTO>>(cachedData);
                if (entries != null)
                {
                    entries.RemoveAll(e => e.Id == id);

                    if (entries.Any())
                    {
                        _cacheService.SaveCache(cacheKey, entries);
                        logger.Info($"Redis cache updated after deleting Entry ID: {id}");
                    }
                    else
                    {
                        _cacheService.DeleteCache(cacheKey);
                        logger.Info($"Redis cache key {cacheKey} deleted as no entries remain.");
                    }
                }
            }

            return entry;
        }

        public List<AddressBookEntity> GetAllContacts()
        {
            return _context.AddressBook.ToList();
        }

        public bool AdminDeleteEntry(int userId, int contactId)
        {
            var entry = _context.AddressBook
                                .FirstOrDefault(e => e.UserId == userId && e.Id == contactId);
            if (entry == null)
            {
                return false;
            }

            _context.AddressBook.Remove(entry);
            _context.SaveChanges();
            return true;
        }
    }
}