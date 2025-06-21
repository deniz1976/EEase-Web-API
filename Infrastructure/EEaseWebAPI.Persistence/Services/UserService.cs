using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.CreateUser;
using EEaseWebAPI.Application.Exceptions.DeleteUser;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Extensions;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EEaseWebAPI.Application.Exceptions.ResetUserPreferences;
using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
using EEaseWebAPI.Application.MapEntities.PreferenceGroups;
using EEaseWebAPI.Application.Exceptions.Friendship;
using Microsoft.Extensions.Caching.Memory;

namespace EEaseWebAPI.Persistence.Services
{
    /// <summary>
    /// Provides comprehensive user management functionality including user operations, preferences, and friendship features.
    /// Handles user creation, updates, deletion, and various user-related operations.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHeaderService _headerService;
        private readonly IMailService _mailService;
        private readonly IGeminiAIService _geminiAIService;
        private readonly EEaseAPIDbContext _context;
        private readonly ICityService _cityService;
        private readonly IUserAccommodationPreferencesReadRepository _userAccommodationPreferencesReadRepository;
        private readonly IUserAccommodationPreferencesWriteRepository _userAccommodationPreferencesWriteRepository;
        private readonly IUserFoodPreferencesWriteRepository _userFoodPreferencesWriteRepository;
        private readonly IUserFoodPreferencesReadRepository _userFoodPreferencesReadRepository;
        private readonly IUserPersonalizationWriteRepository _userPersonalizationWriteRepository;
        private readonly IUserPersonalizationReadRepository _userPersonalizationReadRepository;
        private readonly IMemoryCache _cache;
        private readonly IUserCacheService _userCacheService;

        /// <summary>
        /// Initializes a new instance of the UserService class with required dependencies.
        /// </summary>
        /// <param name="userManager">Manages user-related operations</param>
        /// <param name="headerService">Service for handling response headers</param>
        /// <param name="mailService">Service for email operations</param>
        /// <param name="geminiAIService">AI service for user preference analysis</param>
        /// <param name="context">Database context</param>
        /// <param name="cityService">Service for city-related operations</param>
        /// <param name="userAccommodationPreferencesWriteRepository">Repository for writing accommodation preferences</param>
        /// <param name="userFoodPreferencesWriteRepository">Repository for writing food preferences</param>
        /// <param name="userFoodPreferencesReadRepository">Repository for reading food preferences</param>
        /// <param name="userPersonalizationWriteRepository">Repository for writing personalization settings</param>
        /// <param name="userPersonalizationReadRepository">Repository for reading personalization settings</param>
        /// <param name="userAccommodationPreferencesReadRepository">Repository for reading accommodation preferences</param>
        /// <param name="cache">Memory cache for currency validation</param>
        /// <param name="userCacheService">Service for user cache operations</param>
        public UserService(UserManager<AppUser> userManager, IHeaderService headerService, IMailService mailService, 
            IGeminiAIService geminiAIService, EEaseAPIDbContext context, ICityService cityService, 
            IUserAccommodationPreferencesWriteRepository userAccommodationPreferencesWriteRepository, 
            IUserFoodPreferencesWriteRepository userFoodPreferencesWriteRepository, 
            IUserFoodPreferencesReadRepository userFoodPreferencesReadRepository, 
            IUserPersonalizationWriteRepository userPersonalizationWriteRepository, 
            IUserPersonalizationReadRepository userPersonalizationReadRepository,
            IUserAccommodationPreferencesReadRepository userAccommodationPreferencesReadRepository,
            IMemoryCache cache,
            IUserCacheService userCacheService)
        {
            _userManager = userManager;
            _headerService = headerService;
            _mailService = mailService;
            _geminiAIService = geminiAIService;
            _context = context;
            _cityService = cityService;
            _userAccommodationPreferencesWriteRepository = userAccommodationPreferencesWriteRepository;
            _userFoodPreferencesWriteRepository = userFoodPreferencesWriteRepository;
            _userFoodPreferencesReadRepository = userFoodPreferencesReadRepository;
            _userPersonalizationWriteRepository = userPersonalizationWriteRepository;
            _userPersonalizationReadRepository = userPersonalizationReadRepository;
            _userAccommodationPreferencesReadRepository = userAccommodationPreferencesReadRepository;
            _cache = cache;
            _userCacheService = userCacheService;
        }

        /// <summary>
        /// Creates a new user account with the provided information.
        /// </summary>
        /// <param name="model">User creation model containing necessary information</param>
        /// <returns>Response indicating the success of user creation</returns>
        /// <exception cref="CreateUserFailedException">Thrown when user creation fails</exception>
        public async Task<CreateUserResponse> CreateAsync(CreateUser model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                throw new CreateUserFailedException("This email is already in use", (int)StatusEnum.EmailAlreadyInUse);

            if (await _userManager.FindByNameAsync(model.Username) != null)
                throw new CreateUserFailedException("This username is already in use", (int)StatusEnum.InvalidUsername);

            if (model.BornDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var age = today.Year - model.BornDate.Value.Year;
                
                if (today.Month < model.BornDate.Value.Month || 
                    (today.Month == model.BornDate.Value.Month && today.Day < model.BornDate.Value.Day))
                {
                    age--;
                }

                if (age < 12)
                {
                    throw new CreateUserFailedException("Users must be at least 12 years old to register", (int)StatusEnum.InvalidAge);
                }
            }

            model.Name = CapitalizeFirstLetter(model.Name);
            model.Surname = CapitalizeFirstLetter(model.Surname);

            var verificationCode = new Random().Next(100000, 999999).ToString();

            var newUser = new Domain.Entities.Identity.AppUser
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Email = model.Email,
                Surname = model.Surname,
                UserName = model.Username,
                Gender = model.Gender,
                VerificationCode = verificationCode,
                BornDate = model.BornDate
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new CreateUserFailedException($"Failed to create user: {errors}", (int)StatusEnum.UserUpdateFailed);
            }

            _mailService.SendVerificationEmail(model.Email, "Email Confirm", verificationCode);

            return new CreateUserResponse
            {
                response = new Application.MapEntities.CreateUser.CreateUser
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.SuccessfullyCreated),
                    Body = new Application.MapEntities.CreateUser.CreateUserBody
                    {
                        message = "Account created successfully. You must confirm email before use."
                    }
                }
            };
        }

        public async Task<bool> SendVerificationEmailAgain(string email)
        {
            if (email == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", 7);

            var user = await _userManager.FindByEmailAsync(email);

            if(user.EmailConfirmed) { throw new Application.Exceptions.Login.UserNotFoundException("User already confirmed",3); }

            if (user != null) 
            {
                var verificationCode = new Random().Next(100000, 999999).ToString();
                user.VerificationCode = verificationCode;
                await _context.SaveChangesAsync();
                _mailService.SendVerificationEmail(email, "Email Confirm", verificationCode);
                return true;
            }
            return false;

        }

        /// <summary>
        /// Confirms a user's email address using the provided verification code.
        /// </summary>
        /// <param name="code">The verification code sent to the user's email</param>
        /// <param name="usernameOrEmail">The username or email of the user</param>
        /// <returns>True if confirmation is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="EmailConfirmException">Thrown when email confirmation fails</exception>
        public async Task<bool> EmailConfirm(string code, string usernameOrEmail)
        {
            var user = await _userManager.FindByEmailAsync(usernameOrEmail)
                       ?? await _userManager.FindByNameAsync(usernameOrEmail);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            if (user.EmailConfirmed)
                throw new EEaseWebAPI.Application.Exceptions.EmailConfirmException("Email is already confirmed", (int)StatusEnum.EmailConfirmed);

            if (user.VerificationCode == code)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                _userCacheService.AddOrUpdateUserInCache(user);
                return true;
            }

            throw new EEaseWebAPI.Application.Exceptions.EmailConfirmException("Code is not correct", (int)StatusEnum.InvalidEmailConfirmationCode);
        }

        /// <summary>
        /// Retrieves user information based on the provided username.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>User information including profile details</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<GetUserInfo> GetUserInfoQuery(string username) 
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new Application.Exceptions.UpdateUser.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);
            
            return new GetUserInfo() 
            {
                name = user.Name,
                surname = user.Surname,
                email = user.Email,
                gender = user.Gender,
                username = username,
                borndate = user.BornDate,
                bio = user.Bio,
                currency = user.Currency,
                photoPath = user.PhotoPath,
                country = user.Country,
                id = user.Id
                
            };
        }

        /// <summary>
        /// Updates a user's refresh token and its expiration date.
        /// </summary>
        /// <param name="refreshToken">The new refresh token</param>
        /// <param name="user">The user whose token is being updated</param>
        /// <param name="accessTokenDate">The date when the access token was issued</param>
        /// <param name="addOnAccessTokenDate">Additional time in seconds to add to the token expiration</param>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, DateTime accessTokenDate, int addOnAccessTokenDate)
        {
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            user.RefreshToken = refreshToken;
            user.RefreshTokenEndDate = accessTokenDate.AddSeconds(addOnAccessTokenDate*60*24);

            await _userManager.UpdateAsync(user);
        }

        /// <summary>
        /// Updates user preferences based on a natural language message.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="message">Message containing preference information</param>
        /// <returns>True if update is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="UpdateUserSaveException">Thrown when preference update fails</exception>
        public async Task<bool> UpdateUserPreferences(string username, string message)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new EEaseWebAPI.Application.Exceptions.UpdateUser.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            try
            {
                var existingAccommodationPrefs = await _userAccommodationPreferencesReadRepository.GetSingleAsync(x => x.UserId == user.Id, false);
                var existingFoodPrefs = await _userFoodPreferencesReadRepository.GetSingleAsync(x => x.UserId == user.Id, false);
                var existingPersonalization = await _userPersonalizationReadRepository.GetSingleAsync(x => x.UserId == user.Id, false);

                if (existingAccommodationPrefs != null || existingFoodPrefs != null || existingPersonalization != null)
                {
                    throw new UserAlreadyHasPreferencesException("User already has preferences set. Please use reset preferences first.");
                }

                var (accommodationPreferences, foodPreferences, personalization) = 
                    await _geminiAIService.GetUserPreferencesFromMessage(message);

                bool isAccommodationNull = true;
                bool isFoodNull = true;
                bool isPersonalizationNull = true;

                foreach (var prop in typeof(UserAccommodationPreferences).GetProperties())
                {
                    if (prop.Name != "UserId" && prop.Name != "Id" && prop.Name != "User" && 
                        prop.PropertyType == typeof(int?) && 
                        prop.GetValue(accommodationPreferences) != null)
                    {
                        isAccommodationNull = false;
                        break;
                    }
                }

                foreach (var prop in typeof(UserFoodPreferences).GetProperties())
                {
                    if (prop.Name != "UserId" && prop.Name != "Id" && prop.Name != "User" && 
                        prop.PropertyType == typeof(int?) && 
                        prop.GetValue(foodPreferences) != null)
                    {
                        isFoodNull = false;
                        break;
                    }
                }

                foreach (var prop in typeof(UserPersonalization).GetProperties())
                {
                    if (prop.Name != "UserId" && prop.Name != "Id" && prop.Name != "User" && 
                        prop.PropertyType == typeof(int?) && 
                        prop.GetValue(personalization) != null)
                    {
                        isPersonalizationNull = false;
                        break;
                    }
                }

                if (isAccommodationNull && isFoodNull && isPersonalizationNull)
                {
                    throw new UpdateUserSaveException("Preferences could not be extracted from your message. Please provide more specific details about your preferences.", (int)StatusEnum.PreferencesUpdateFailed);
                }

                accommodationPreferences.UserId = user.Id;
                foodPreferences.UserId = user.Id;
                personalization.UserId = user.Id;

                await _userAccommodationPreferencesWriteRepository.AddAsync(accommodationPreferences);
                await _userFoodPreferencesWriteRepository.AddAsync(foodPreferences);
                await _userPersonalizationWriteRepository.AddAsync(personalization);

                await _userAccommodationPreferencesWriteRepository.SaveAsync();
                await _userFoodPreferencesWriteRepository.SaveAsync();
                await _userPersonalizationWriteRepository.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new UpdateUserSaveException($"Failed to update user preferences: {ex.Message}", (int)StatusEnum.PreferencesUpdateFailed);
            }
        }

        /// <summary>
        /// Resets all user preferences to their default values.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>True if reset is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="ResetUserPreferencesFailedException">Thrown when preference reset fails</exception>
        public async Task<bool> ResetUserPreferences(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    throw new EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException($"User with username {username} not found", (int)StatusEnum.UserNotFound);

                var accommodationPrefs = await _userAccommodationPreferencesReadRepository.GetSingleAsync(x => x.UserId == user.Id, true);
                var foodPrefs = await _userFoodPreferencesReadRepository.GetSingleAsync(x => x.UserId == user.Id, true);
                var personalPrefs = await _userPersonalizationReadRepository.GetSingleAsync(x => x.UserId == user.Id, true);

                if (accommodationPrefs == null && foodPrefs == null && personalPrefs == null)
                    throw new ResetUserPreferencesFailedException("User has no preferences to reset");

                if (accommodationPrefs != null)
                    _userAccommodationPreferencesWriteRepository.Remove(accommodationPrefs);

                if (foodPrefs != null)
                    _userFoodPreferencesWriteRepository.Remove(foodPrefs);

                if (personalPrefs != null)
                    _userPersonalizationWriteRepository.Remove(personalPrefs);

                await _userAccommodationPreferencesWriteRepository.SaveAsync();
                await _userFoodPreferencesWriteRepository.SaveAsync();
                await _userPersonalizationWriteRepository.SaveAsync();

                return true;
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new ResetUserPreferencesFailedException($"Failed to reset user preferences: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a user's country information.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="country">The new country value</param>
        /// <returns>True if update is successful</returns>
        /// <exception cref="InvalidCountryException">Thrown when the country is not valid</exception>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<bool> UpdateUserCountry(string username, string country)
        {
            try
            {
                var availableCountries = await _cityService.GetAllCountries();

                if (!availableCountries.Contains(country, StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidCountryException($"Country '{country}' is not in the available countries list.");
                }

                AppUser? user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    throw new EEaseWebAPI.Application.Exceptions.UpdateUser.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

                user.Country = country;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to update user country: {errors}");
                }

                return true;
            }
            catch (InvalidCountryException)
            {
                throw;
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new Exception($"Failed to update user country: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelFriendRequest(string username, string targetUsername)
        {
            var user = await _userManager.FindByNameAsync(username);
            var targetUser = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || targetUser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            var friendship = await _context.Set<UserFriendship>()
                .FirstOrDefaultAsync(f =>
                    f.RequesterId == user.Id &&
                    f.AddresseeId == targetUser.Id &&
                    f.Status == FriendshipStatus.Pending);

            if (friendship == null)
                throw new FriendshipNotFoundException();

            _context.Set<UserFriendship>().Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FriendRequestStatus> CheckFriendRequest(string username, string targetUsername) 
        {
            if (username == targetUsername)
                return FriendRequestStatus.AlreadyFriends;

            var user = await _userManager.FindByNameAsync(username);
            var targetUser = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || targetUser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            var friendship = await _context.Set<UserFriendship>()
                .FirstOrDefaultAsync(f =>
                    (f.RequesterId == user.Id && f.AddresseeId == targetUser.Id) ||
                    (f.RequesterId == targetUser.Id && f.AddresseeId == user.Id));

            if (friendship == null)
                return FriendRequestStatus.NoRequest;

            if (friendship.Status == FriendshipStatus.Blocked)
                return FriendRequestStatus.Blocked;

            if (friendship.Status == FriendshipStatus.Accepted)
                return FriendRequestStatus.AlreadyFriends;

            if (friendship.Status == FriendshipStatus.Pending)
            {
                if (friendship.RequesterId == user.Id)
                    return FriendRequestStatus.Requester;
                else
                    return FriendRequestStatus.Addressee;
            }

            return FriendRequestStatus.NoRequest;
        }

        /// <summary>
        /// Retrieves a user's friendship status with another user.
        /// </summary>
        /// <param name="requesterUsername">The username of the requesting user</param>
        /// <param name="AddresseeUsername">The username of the target user</param>
        /// <returns>The friendship details between the users</returns>
        /// <exception cref="UserNotFoundException">Thrown when either user is not found</exception>
        public async Task<UserFriendship> GetFriendshipAsync(string requesterUsername, string AddresseeUsername)
        {
            var requester = await _userManager.FindByNameAsync(requesterUsername);
            var addressee = await _userManager.FindByNameAsync(AddresseeUsername);

            if (requester == null || addressee == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            return await _context.Set<UserFriendship>()
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == requester.Id && f.AddresseeId == addressee.Id) ||
                    (f.RequesterId == addressee.Id && f.AddresseeId == requester.Id));
        }

        /// <summary>
        /// Retrieves a user's complete profile including all preferences.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>The user entity with all related preferences</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<AppUser> GetUserWithPreferencesAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.UserPersonalization)
                .Include(u => u.FoodPreferences)
                .Include(u => u.AccommodationPreferences)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                throw new EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            return user;
        }

        /// <summary>
        /// Capitalizes the first letter of a string and converts the rest to lowercase.
        /// </summary>
        /// <param name="input">The string to process</param>
        /// <returns>The processed string with first letter capitalized</returns>
        private string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        /// <summary>
        /// Checks if a user's email is confirmed.
        /// </summary>
        /// <param name="emailOrUsername">The email or username of the user</param>
        /// <returns>True if email is confirmed, false otherwise</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<bool> CheckEmailConfirmed(string emailOrUsername)
        {
            var user = await _userManager.FindByEmailAsync(emailOrUsername)
                       ?? await _userManager.FindByNameAsync(emailOrUsername);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            return user.EmailConfirmed;
        }

        /// <summary>
        /// Updates user information based on the provided request.
        /// </summary>
        /// <param name="request">Request containing updated user information</param>
        /// <returns>True if update is successful</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
        /// <exception cref="ArgumentException">Thrown when validation fails for username, name, surname, gender, or birth date</exception>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="UpdateUserSaveException">Thrown when update operation fails</exception>
        public async Task<bool> UpdateUser(UpdateUserCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.user))
                throw new UnauthorizedAccessException("User is not authenticated.");

            if(request.Username != null)
            {
                if (!await IsUsernameUnique(request.Username))
                    throw new ArgumentException("Username must be unique.");
            }
            

            if(request.Name != null)
            {
                if ((request.Name.Length < 2 || request.Name.Length > 16))
                    throw new ArgumentException("Name must be between 2 and 16 characters.");
            }

            if (request.bio != null) 
            {
                if (request.bio.Length < 0 || request.bio.Length > 80) 
                {
                    throw new ArgumentException("Bio must be between 0 and 80 characters.");
                }
            }
            

            if(request.Surname != null)
            {
                if ((request.Surname.Length < 2 || request.Surname.Length > 16))
                    throw new ArgumentException("Surname must be between 2 and 16 characters.");
            }

            if(request.Gender != "Male")
                if(request.Gender != "Female")
                    if(request.Gender != null)
                    throw new ArgumentException("Gender must be 'Male' or 'Female'.");
                

            if (request.BornDate != null)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var minimumBirthDate = today.AddYears(-13);

                if (request.BornDate > minimumBirthDate)
                    throw new ArgumentException("User must be at least 13 years old.");
            }

            var user = await GetUserByUsername(request.user);
            if (user == null)
                throw new Application.Exceptions.UpdateUser.UserNotFoundException("User not found.", (int)StatusEnum.UserNotFound);

            user.UserName = request.Username ?? user.UserName;
            user.Name = request.Name ?? user.Name;
            user.Surname = request.Surname ?? user.Surname;
            user.Gender = request.Gender ?? user.Gender;
            user.BornDate = request.BornDate ?? user.BornDate;
            user.Bio = request.bio ?? user.Bio;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new UpdateUserSaveException($"Failed to change the password. Errors: {errors}", (int)StatusEnum.UserUpdateFailed);
            }
            else
            {
                _userCacheService.UpdateUserAttributesInCache(user.Id, request.Username, request.Name, request.Surname);
                return true;
            }
        }

        /// <summary>
        /// Checks if a username is unique in the system.
        /// </summary>
        /// <param name="username">The username to check</param>
        /// <returns>True if username is unique, false otherwise</returns>
        private async Task<bool> IsUsernameUnique(string username)
        {
            return await _userManager.Users.AllAsync(u => u.UserName != username);
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>The user if found, null otherwise</returns>
        private async Task<AppUser?> GetUserByUsername(string username)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
        }

        /// <summary>
        /// Initiates the user deletion process by sending a verification code via email.
        /// </summary>
        /// <param name="username">The username of the user to delete</param>
        /// <returns>True if deletion code is sent, false if user is already marked for deletion</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<bool> DeleteUserSendMail(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            if (user.status == false)
            {
                user.status = true;
                user.DeleteDate = null;
                await _userManager.UpdateAsync(user);
                return false;
            }

            var code = new Random().Next(100000, 999999).ToString();
            user.DeleteCode = code;

            await _userManager.UpdateAsync(user);

            _mailService.SendDeleteCodeEmail(user.Email, "Delete Account", code);

            return true;
        }

        /// <summary>
        /// Processes user deletion with a verification code.
        /// </summary>
        /// <param name="username">The username of the user to delete</param>
        /// <param name="code">The verification code sent to user's email</param>
        /// <returns>A message indicating the deletion status</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="UserStatusAlreadyFalseException">Thrown when user is already marked for deletion</exception>
        /// <exception cref="DeleteUserCodeNotCorrectException">Thrown when verification code is incorrect</exception>
        public async Task<string> DeleteUserWithCode(string username, string code)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found.", (int)StatusEnum.UserNotFound);

            if (user.DeleteCode == code && user.status == true)
            {
                user.DeleteDate = DateTime.UtcNow.AddDays(7);
                user.status = false; 
                user.DeleteCode = null; 
                await _userManager.UpdateAsync(user);

                return "Code is correct, account will be deleted in 7 days";
            }

            if (user.status == false)
                throw new UserStatusAlreadyFalseException("User status already false", (int)StatusEnum.UserDeletionFailed);

            throw new DeleteUserCodeNotCorrectException("Delete user code is wrong.", (int)StatusEnum.InvalidDeleteCode);
        }

        /// <summary>
        /// Checks the current status of a user's account.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>Status information including active/passive state and message</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<StatusCheckBody> StatusCheck(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if(user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found.", (int)StatusEnum.UserNotFound);

            string message = (bool)user.status ? "User is active" : "User is passive";

            return new StatusCheckBody()
            {
                status = user.status,
                message = message 
            };
        }

        /// <summary>
        /// Retrieves descriptions of user preferences in a human-readable format.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>Detailed descriptions of user's preferences</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<GetUserPreferenceDescriptionsBody> GetUserPreferenceDescriptionsAsync(string username)
        {
            try
            {
                var user = await GetUserWithPreferencesAsync(username);
                var response = new GetUserPreferenceDescriptionsBody();

                if (user.UserPersonalization != null)
                {
                    foreach (PersonalizationTypes type in Enum.GetValues(typeof(PersonalizationTypes)))
                    {
                        var propertyName = $"{type}Preference";
                        var propertyInfo = typeof(UserPersonalization).GetProperty(propertyName);
                        if (propertyInfo != null)
                        {
                            var value = (int?)propertyInfo.GetValue(user.UserPersonalization);
                            if (value.HasValue && value > 45)
                            {
                                response.PersonalizationPreferences.Add(new PreferenceDetail
                                {
                                    Description = type.GetDescription(),
                                    Value = value.Value
                                });
                            }
                        }
                    }
                }

                if (user.FoodPreferences != null)
                {
                    foreach (FoodPreferenceTypes type in Enum.GetValues(typeof(FoodPreferenceTypes)))
                    {
                        var propertyName = $"{type}Preference";
                        var propertyInfo = typeof(UserFoodPreferences).GetProperty(propertyName);
                        if (propertyInfo != null)
                        {
                            var value = (int?)propertyInfo.GetValue(user.FoodPreferences);
                            if (value.HasValue && value > 45)
                            {
                                response.FoodPreferences.Add(new PreferenceDetail
                                {
                                    Description = type.GetDescription(),
                                    Value = value.Value
                                });
                            }
                        }
                    }
                }

                if (user.AccommodationPreferences != null)
                {
                    foreach (AccommodationPreferenceTypes type in Enum.GetValues(typeof(AccommodationPreferenceTypes)))
                    {
                        var propertyName = $"{type}Preference";
                        var propertyInfo = typeof(UserAccommodationPreferences).GetProperty(propertyName);
                        if (propertyInfo != null)
                        {
                            var value = (int?)propertyInfo.GetValue(user.AccommodationPreferences);
                            if (value.HasValue && value > 45)
                            {
                                response.AccommodationPreferences.Add(new PreferenceDetail
                                {
                                    Description = type.GetDescription(),
                                    Value = value.Value
                                });
                            }
                        }
                    }
                }

                if (!response.PersonalizationPreferences.Any() && 
                    !response.FoodPreferences.Any() && 
                    !response.AccommodationPreferences.Any())
                {
                    throw new BaseException("No preferences found with value greater than 45", (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
                }

                return response;
            }
            catch (EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BaseException($"Failed to retrieve preference descriptions: {ex.Message}", (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
            }
        }

        /// <summary>
        /// Retrieves all available preference topics for users.
        /// </summary>
        /// <returns>A collection of topics categorized by accommodation, food, and travel preferences</returns>
        public GetAllTopicsQueryResponseBody GetAllTopics() 
        {
            return new GetAllTopicsQueryResponseBody
            {
                AccommodationTopics = [.. TravelPreferenceGroups.AccommodationGroups.Groups.Keys],
                FoodTopics = [.. TravelPreferenceGroups.FoodGroups.Groups.Keys],
                TravelTopics = [.. TravelPreferenceGroups.TravelGroups.Groups.Keys]
            };
        }

        /// <summary>
        /// Updates user preferences based on selected topics.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="topics">List of selected preference topics</param>
        /// <returns>True if update is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="UserAlreadyHasPreferencesException">Thrown when user already has preferences set</exception>
        /// <exception cref="UpdateUserSaveException">Thrown when preference update fails</exception>
        public async Task<bool> UpdateUserPreferencesWithTopics(string username, List<string> topics)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            try
            {
                var existingAccommodationPrefs = await _userAccommodationPreferencesReadRepository.GetSingleAsync(x => x.UserId == user.Id, false);
                var existingFoodPrefs = await _userFoodPreferencesReadRepository.GetSingleAsync(x => x.UserId == user.Id, false);
                var existingPersonalization = await _userPersonalizationReadRepository.GetSingleAsync(x => x.UserId == user.Id, false);

                if (existingAccommodationPrefs != null || existingFoodPrefs != null || existingPersonalization != null)
                {
                    throw new UserAlreadyHasPreferencesException("User already has preferences set. Please use reset preferences first.");
                }

                var accommodationPreferences = new UserAccommodationPreferences { UserId = user.Id };
                var foodPreferences = new UserFoodPreferences { UserId = user.Id };
                var personalization = new UserPersonalization { UserId = user.Id };

                foreach (var topic in topics)
                {
                    if (TravelPreferenceGroups.AccommodationGroups.Groups.Any(g => g.Key == topic))
                    {
                        var preferences = TravelPreferenceGroups.AccommodationGroups.Groups[topic];
                        foreach (var pref in preferences)
                        {
                            var propertyInfo = typeof(UserAccommodationPreferences).GetProperty(pref);
                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue(accommodationPreferences, 60);
                            }
                        }
                    }
                    else if (TravelPreferenceGroups.FoodGroups.Groups.Any(g => g.Key == topic))
                    {
                        var preferences = TravelPreferenceGroups.FoodGroups.Groups[topic];
                        foreach (var pref in preferences)
                        {
                            var propertyInfo = typeof(UserFoodPreferences).GetProperty(pref);
                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue(foodPreferences, 60);
                            }
                        }
                    }
                    else if (TravelPreferenceGroups.TravelGroups.Groups.Any(g => g.Key == topic))
                    {
                        var preferences = TravelPreferenceGroups.TravelGroups.Groups[topic];
                        foreach (var pref in preferences)
                        {
                            var propertyInfo = typeof(UserPersonalization).GetProperty(pref);
                            if (propertyInfo != null)
                            {
                                propertyInfo.SetValue(personalization, 60);
                            }
                        }
                    }
                }

                await _userAccommodationPreferencesWriteRepository.AddAsync(accommodationPreferences);
                await _userFoodPreferencesWriteRepository.AddAsync(foodPreferences);
                await _userPersonalizationWriteRepository.AddAsync(personalization);

                await _userAccommodationPreferencesWriteRepository.SaveAsync();
                await _userFoodPreferencesWriteRepository.SaveAsync();
                await _userPersonalizationWriteRepository.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new UpdateUserSaveException($"Failed to update user preferences with topics: {ex.Message}", (int)StatusEnum.PreferencesUpdateFailed);
            }
        }

        /// <summary>
        /// Retrieves a list of user's friends.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>Collection of accepted friendships</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<IEnumerable<UserFriendship>> GetUserFriendsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            var result =  await _context.Set<UserFriendship>()
                .Where(f => 
                    (f.RequesterId == user.Id || f.AddresseeId == user.Id) &&
                    f.Status == FriendshipStatus.Accepted)
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// Retrieves pending friend requests for a user.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>Collection of pending friend requests</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<IEnumerable<UserFriendship>> GetPendingFriendRequestsAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if(user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);
            return await _context.Set<UserFriendship>()
                .Where(f => 
                    f.AddresseeId == user.Id &&
                    f.Status == FriendshipStatus.Pending)
                .Include(f => f.Requester)
                .ToListAsync();
        }

        /// <summary>
        /// Removes a friendship between users.
        /// </summary>
        /// <param name="friendship">The friendship to remove</param>
        /// <returns>True if removal is successful</returns>
        public async Task<bool> RemoveFriendshipAsync(UserFriendship friendship)
        {
            _context.Set<UserFriendship>().Remove(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Unblocks a previously blocked user.
        /// </summary>
        /// <param name="targetUsername">The username of the user to unblock</param>
        /// <param name="username">The username of the user performing the unblock</param>
        /// <returns>True if unblock is successful</returns>
        /// <exception cref="ArgumentNullException">Thrown when username parameters are null or empty</exception>
        /// <exception="UserNotFoundException">Thrown when either user is not found</exception>
        /// <exception cref="FriendshipNotFoundException">Thrown when no blocking relationship exists</exception>
        public async Task<bool> UnblockUserAsync(string targetUsername, string username)
        {
            if (string.IsNullOrEmpty(targetUsername) || string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty");

            var user = await _userManager.FindByNameAsync(username);
            var targetUser = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || targetUser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            UserFriendship? friendship = await _context.Set<UserFriendship>()
                .FirstOrDefaultAsync(f =>f.AddresseeId == targetUser.Id && f.RequesterId == user.Id);

            if (friendship == null)
            {
                throw new FriendshipNotFoundException();
            }
            else
            {
                _context.Set<UserFriendship>().Remove(friendship);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Blocks a user.
        /// </summary>
        /// <param name="targetUsername">The username of the user to block</param>
        /// <param name="username">The username of the user performing the block</param>
        /// <returns>True if block is successful</returns>
        /// <exception cref="ArgumentNullException">Thrown when username parameters are null or empty</exception>
        /// <exception cref="UserNotFoundException">Thrown when either user is not found</exception>
        /// <exception cref="UserAlreadyBlockedException">Thrown when user is already blocked</exception>
        public async Task<bool> BlockUserAsync(string targetUsername, string username)
        {
            if (string.IsNullOrEmpty(targetUsername) || string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username), "Username cannot be null or empty");

            var user = await _userManager.FindByNameAsync(username);
            var targetUser = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || targetUser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            UserFriendship? friendship = await _context.Set<UserFriendship>()
                .FirstOrDefaultAsync(f =>
                    (f.AddresseeId == user.Id && f.RequesterId == targetUser.Id) ||
                    (f.AddresseeId == targetUser.Id && f.RequesterId == user.Id));


            if (friendship != null && friendship.Status == FriendshipStatus.Blocked) 
            {
                if(friendship.RequesterId == user.Id) 
                {
                    throw new UserAlreadyBlockedException();
                }

                else 
                {
                    throw new Exception();
                }
            }
                

            else if (friendship == null)
            {
                var newFriendship = new UserFriendship
                {
                    RequesterId = user.Id,
                    AddresseeId = targetUser.Id,
                    Status = FriendshipStatus.Blocked,
                    RequestDate = DateTime.UtcNow
                };

                await _context.Set<UserFriendship>().AddAsync(newFriendship);

            }
            else
            {
                friendship.Status = FriendshipStatus.Blocked;
                friendship.RequesterId = user.Id;
                friendship.AddresseeId = targetUser.Id;
            }

            await _context.SaveChangesAsync();
            return true;

        }

        /// <summary>
        /// Retrieves a list of users blocked by the specified user.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>Collection of blocked users</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        public async Task<IEnumerable<UserFriendship>> GetBlockedUsersAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            return await _context.Set<UserFriendship>()
                .Where(f => 
                    f.RequesterId == user.Id && 
                    f.Status == FriendshipStatus.Blocked)
                .Include(f => f.Requester)
                .Include(f => f.Addressee)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new friendship relationship.
        /// </summary>
        /// <param name="friendship">The friendship details to create</param>
        /// <returns>True if creation is successful</returns>
        public async Task<bool> CreateFriendshipAsync(UserFriendship friendship)
        {
            await _context.Set<UserFriendship>().AddAsync(friendship);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Creates a new friendship request between two users.
        /// </summary>
        /// <param name="requesterUsername">The username of the user sending the request</param>
        /// <param name="AddresseeUsername">The username of the user receiving the request</param>
        /// <returns>True if request creation is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when either user is not found</exception>
        public async Task<bool> CreateFriendshipAsync(string requesterUsername, string AddresseeUsername) 
        {
            var requester = await _userManager.FindByNameAsync(requesterUsername);
            var addressee = await _userManager.FindByNameAsync(AddresseeUsername);

            if (requester == null || addressee == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            var newFriendship = new UserFriendship
            {
                RequesterId = requester.Id,
                AddresseeId = addressee.Id,
                Status = FriendshipStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            return await CreateFriendshipAsync(newFriendship);
        }

        /// <summary>
        /// Updates the status of a friendship between users.
        /// </summary>
        /// <param name="requesterName">The username of the request sender</param>
        /// <param name="addresseeName">The username of the request receiver</param>
        /// <param name="newStatus">The new status to set for the friendship</param>
        /// <returns>True if status update is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when either user is not found</exception>
        public async Task<bool> UpdateFriendshipStatusAsync(string requesterName, string addresseeName, FriendshipStatus newStatus)
        { 
            var requester = await _userManager.FindByNameAsync(requesterName);
            var addressee = await _userManager.FindByNameAsync(addresseeName);

            if (requester == null || addressee == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            var friendship = await GetFriendshipAsync(requesterName, addresseeName);
            if (friendship == null)
                return false;

            if (newStatus == FriendshipStatus.Blocked)
            {
                _context.Set<UserFriendship>().Remove(friendship);
                
                var newFriendship = new UserFriendship
                {
                    RequesterId = requester.Id, 
                    AddresseeId = addressee.Id,
                    Status = FriendshipStatus.Blocked,
                    RequestDate = DateTime.UtcNow,
                    ResponseDate = DateTime.UtcNow
                };
                
                await _context.Set<UserFriendship>().AddAsync(newFriendship);
            }
            else
            {
                friendship.Status = newStatus;
                friendship.ResponseDate = DateTime.UtcNow;
                _context.Set<UserFriendship>().Update(friendship);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Checks if two users are friends by checking their friendship status in the database.
        /// </summary>
        /// <param name="username1">First user's username</param>
        /// <param name="username2">Second user's username</param>
        /// <returns>True if users are friends, false otherwise</returns>
        public async Task<bool> IsFriendAsync(string username1, string username2)
        {
            var user1 = await _userManager.FindByNameAsync(username1);
            var user2 = await _userManager.FindByNameAsync(username2);

            if (user1 == null || user2 == null)
                return false;

            var friendship = await _context.UserFriendships
                .FirstOrDefaultAsync(f => 
                    (f.RequesterId == user1.Id && f.AddresseeId == user2.Id) || 
                    (f.RequesterId == user2.Id && f.AddresseeId == user1.Id));

            return friendship != null && friendship.Status == Domain.Enums.FriendshipStatus.Accepted;
        }

        /// <summary>
        /// Updates the user's currency preference with the specified currency code.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="currencyCode">Three-letter currency code (e.g., USD, EUR)</param>
        /// <returns>True if update is successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="ArgumentException">Thrown when currency code is invalid</exception>
        public async Task<bool> UpdateUserCurrency(string username, string currencyCode)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(currencyCode))
                throw new ArgumentNullException("Username and currency code cannot be null or empty");

            var user = await GetUserByUsername(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            const string cacheKey = "AllCurrencies_Cache";
            if (!_cache.TryGetValue(cacheKey, out List<Domain.Entities.Currency.AllWordCurrencies> currencies))
            {
                throw new Exception("Currency data not available in cache");
            }

            var isValidCurrency = currencies.Any(c => c.AlphabeticCode.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));
            if (!isValidCurrency)
                throw new Exception("Invalid currency code");

            user.Currency = currencyCode;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<string> GetUserCurrencyAsync(string username)
        {
            if(string.IsNullOrEmpty(username))
                throw new ArgumentNullException("username");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null || user.Currency == null)
                throw new ArgumentNullException();

            return user.Currency;
        }

        public async Task<string> GetUserPhotoAsync(string username) 
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);

            return user.PhotoPath;
        }

        /// <summary>
        /// Checks if a user is blocked by another user.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="targetUsername">Username of the target user</param>
        /// <returns>True if user is blocked by target, false otherwise</returns>
        public async Task<bool> IsBlockedByUserAsync(string username, string targetUsername)
        {
            var user = await _userManager.FindByNameAsync(username);
            var targetUser = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || targetUser == null)
                return false;

            var friendship = await _context.UserFriendships
                .FirstOrDefaultAsync(f => 
                    f.RequesterId == targetUser.Id && 
                    f.AddresseeId == user.Id && 
                    f.Status == Domain.Enums.FriendshipStatus.Blocked);

            return friendship != null;
        }
        
        /// <summary>
        /// Checks if a user has blocked another user.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="targetUsername">Username of the target user</param>
        /// <returns>True if user has blocked target, false otherwise</returns>
        public async Task<bool> HasBlockedUserAsync(string username, string targetUsername)
        {
            var user = await _userManager.FindByNameAsync(username);
            var targetUser = await _userManager.FindByNameAsync(targetUsername);

            if (user == null || targetUser == null)
                return false;

            var friendship = await _context.UserFriendships
                .FirstOrDefaultAsync(f => 
                    f.RequesterId == user.Id && 
                    f.AddresseeId == targetUser.Id && 
                    f.Status == Domain.Enums.FriendshipStatus.Blocked);

            return friendship != null;
        }
        
        /// <summary>
        /// Gets the profile visibility status between two users.
        /// </summary>
        /// <param name="username">Username of the viewing user</param>
        /// <param name="targetUsername">Username of the profile owner</param>
        /// <returns>ProfileVisibilityStatus indicating the level of access</returns>
        public async Task<ProfileVisibilityStatus> GetProfileVisibilityStatusAsync(string username, string targetUsername)
        {
            if (username == targetUsername)
                return ProfileVisibilityStatus.FullAccess;
                
            if (await IsBlockedByUserAsync(username, targetUsername))
                return ProfileVisibilityStatus.BlockedByTarget;
                
            if (await HasBlockedUserAsync(username, targetUsername))
                return ProfileVisibilityStatus.BlockedTarget;
                
            if (await IsFriendAsync(username, targetUsername))
                return ProfileVisibilityStatus.FullAccess;
                
            return ProfileVisibilityStatus.LimitedAccess;
        }
        
        /// <summary>
        /// Gets limited user information by username for profile viewing.
        /// </summary>
        /// <param name="username">Username of the viewing user</param>
        /// <param name="targetUsername">Username of the profile owner</param>
        /// <returns>User information with appropriate visibility based on relationship</returns>
        public async Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByNameAsync(string username, string targetUsername)
        {
            var targetUser = await _userManager.FindByNameAsync(targetUsername);
            
            if (targetUser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);
                
            var visibilityStatus = await GetProfileVisibilityStatusAsync(username, targetUsername);

            var checkFriendRequest = await CheckFriendRequest(username,targetUsername);
            
            var userInfo = new GetUserInfo()
            {
                username = targetUser.UserName,
                name = targetUser.Name,
                surname = targetUser.Surname,
                photoPath = targetUser.PhotoPath,
                gender  = targetUser.Gender,
                country = targetUser.Country,
                bio = targetUser.Bio,
                friendRequestStatus = checkFriendRequest,
                Id = targetUser.Id
            };
            
            if (visibilityStatus == ProfileVisibilityStatus.FullAccess)
            {
                userInfo.email = targetUser.Email;
                userInfo.borndate = targetUser.BornDate;
                userInfo.currency = targetUser.Currency;
            }
            
            return (userInfo, visibilityStatus);
        }
        
        /// <summary>
        /// Gets limited user information by ID for profile viewing.
        /// </summary>
        /// <param name="username">Username of the viewing user</param>
        /// <param name="targetUserId">User ID of the profile owner</param>
        /// <returns>User information with appropriate visibility based on relationship</returns>
        public async Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(string username, string targetUserId)
        {
            var targetUser = await _userManager.FindByIdAsync(targetUserId);
            
            if (targetUser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);
                
            return await GetUserInfoByNameAsync(username, targetUser.UserName);
        }
        
        /// <summary>
        /// Retrieves preference descriptions for a specific user by username.
        /// </summary>
        /// <param name="viewerUsername">Username of the viewing user</param>
        /// <param name="targetUsername">Username of the user whose preferences to retrieve</param>
        /// <returns>User preference descriptions if allowed to view, null otherwise</returns>
        public async Task<GetUserPreferenceDescriptionsBody?> GetUserPreferenceDescriptionsByUsernameAsync(string viewerUsername, string targetUsername)
        {
            if (viewerUsername == targetUsername || await IsFriendAsync(viewerUsername, targetUsername))
            {
                try
                {
                    return await GetUserPreferenceDescriptionsAsync(targetUsername);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            
            return null;
        }
        public Task<bool> SetUserPhoto(string username, string photoPath)
        {
            var user = _userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found", (int)StatusEnum.UserNotFound);
            }

            user.Result.PhotoPath = photoPath;
            var result = _userManager.UpdateAsync(user.Result);
            if (result.Result.Succeeded)
            {
                _userCacheService.UpdateUserAttributesInCache(user.Result.Id, photoUrl: photoPath);
                return Task.FromResult(true);
            }
            else
            {
                throw new UpdateUserSaveException("Failed to update user photo", (int)StatusEnum.UserUpdateFailed);
            }
        }
    }
}
