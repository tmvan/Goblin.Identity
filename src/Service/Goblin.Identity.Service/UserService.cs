﻿using System.Collections.Generic;
using System.Linq;
using Elect.DI.Attributes;
using Goblin.Identity.Contract.Repository.Interfaces;
using Goblin.Identity.Contract.Service;
using System.Threading;
using System.Threading.Tasks;
using Elect.Core.StringUtils;
using Elect.Mapper.AutoMapper.IQueryableUtils;
using Elect.Mapper.AutoMapper.ObjUtils;
using Goblin.Core.DateTimeUtils;
using Goblin.Core.Errors;
using Goblin.Identity.Contract.Repository.Models;
using Goblin.Identity.Core;
using Goblin.Identity.Share;
using Goblin.Identity.Share.Models;
using Microsoft.EntityFrameworkCore;

namespace Goblin.Identity.Service
{
    [ScopedDependency(ServiceType = typeof(IUserService))]
    public class UserService : Base.Service, IUserService
    {
        private readonly IGoblinRepository<UserEntity> _userRepo;

        public UserService(IGoblinUnitOfWork goblinUnitOfWork, IGoblinRepository<UserEntity> userRepo) : base(
            goblinUnitOfWork)
        {
            _userRepo = userRepo;
        }

        public async Task<GoblinIdentityEmailConfirmationModel> RegisterAsync(GoblinIdentityRegisterModel model,
            CancellationToken cancellationToken = default)
        {
            CheckUniqueEmail(model.Email);

            CheckUniqueUserName(model.UserName);

            var userEntity = model.MapTo<UserEntity>();

            userEntity.PasswordLastUpdatedTime = GoblinDateTimeHelper.SystemTimeNow;

            userEntity.PasswordHash =
                PasswordHelper.HashPassword(model.Password, userEntity.PasswordLastUpdatedTime);

            userEntity.EmailConfirmToken = StringHelper.Generate(6, false, false);

            userEntity.EmailConfirmTokenExpireTime =
                GoblinDateTimeHelper.SystemTimeNow.Add(SystemSetting.Current.EmailConfirmTokenLifetime);

            _userRepo.Add(userEntity);

            await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);

            var emailConfirmationModel = new GoblinIdentityEmailConfirmationModel
            {
                Id = userEntity.Id,
                EmailConfirmToken = userEntity.EmailConfirmToken,
                EmailConfirmTokenExpireTime = userEntity.EmailConfirmTokenExpireTime
            };

            return emailConfirmationModel;
        }

        public async Task<GoblinIdentityUserModel> GetProfileAsync(long id, CancellationToken cancellationToken = default)
        {
            var userModel =
                await _userRepo
                    .Get(x => x.Id == id)
                    .QueryTo<GoblinIdentityUserModel>()
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(true);

            return userModel;
        }

        public async Task UpdateProfileAsync(long id, GoblinIdentityUpdateProfileModel model,
            CancellationToken cancellationToken = default)
        {
            var userEntity = await _userRepo.Get(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(true);

            if (userEntity == null)
            {
                throw new GoblinException(nameof(GoblinIdentityErrorCode.UserNotFound), GoblinIdentityErrorCode.UserNotFound);
            }

            model.MapTo(userEntity);

            _userRepo.Update(userEntity,
                x => x.AvatarUrl,
                x => x.FullName,
                x => x.Bio,
                x => x.GithubId,
                x => x.SkypeId,
                x => x.FacebookId,
                x => x.WebsiteUrl,
                x => x.CompanyName,
                x => x.CompanyUrl
            );

            await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
        }

        public async Task<GoblinIdentityEmailConfirmationModel> UpdateIdentityAsync(long id,
            GoblinIdentityUpdateIdentityModel model,
            CancellationToken cancellationToken = default)
        {
            var userEntity = await _userRepo.Get(x => x.Id == id)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(true);

            if (userEntity == null)
            {
                throw new GoblinException(nameof(GoblinIdentityErrorCode.UserNotFound), GoblinIdentityErrorCode.UserNotFound);
            }

            var emailConfirmationModel = new GoblinIdentityEmailConfirmationModel();
            
            var changedProperties = new List<string>();

            // Update Password
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                userEntity.PasswordLastUpdatedTime = GoblinDateTimeHelper.SystemTimeNow;
                changedProperties.Add(nameof(userEntity.PasswordLastUpdatedTime));

                userEntity.PasswordHash = PasswordHelper.HashPassword(model.NewPassword, userEntity.PasswordLastUpdatedTime);
                changedProperties.Add(nameof(userEntity.PasswordHash));
            }

            // Update Email
            if (!string.IsNullOrWhiteSpace(model.NewEmail))
            {
                userEntity.EmailConfirmToken = StringHelper.Generate(6, false, false);
                changedProperties.Add(nameof(userEntity.EmailConfirmToken));

                userEntity.EmailConfirmTokenExpireTime = GoblinDateTimeHelper.SystemTimeNow.Add(SystemSetting.Current.EmailConfirmTokenLifetime);
                changedProperties.Add(nameof(userEntity.EmailConfirmTokenExpireTime));

                
                // Email Confirmation Token

                emailConfirmationModel.Id = userEntity.Id;
                
                emailConfirmationModel.EmailConfirmToken = userEntity.EmailConfirmToken;
                
                emailConfirmationModel.EmailConfirmTokenExpireTime = userEntity.EmailConfirmTokenExpireTime;
            }
            
            // Update UserName
            if (!string.IsNullOrWhiteSpace(model.NewUserName))
            {
                userEntity.UserName = model.NewUserName;
                changedProperties.Add(nameof(userEntity.UserName));
            }

            if (!changedProperties.Any())
            {
                return emailConfirmationModel;
            }
            
            _userRepo.Update(userEntity, changedProperties.ToArray());
                
            await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);

            return emailConfirmationModel;
        }

        public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
        {
            var userEntity =
                await _userRepo
                    .Get(x => x.Id == id)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(true);

            if (userEntity == null)
            {
                return;
            }
            
            _userRepo.Delete(userEntity);

            await GoblinUnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
        }

        public async Task<string> GenerateAccessTokenAsync(GoblinIdentityGenerateAccessTokenModel model, CancellationToken cancellationToken = default)
        {
            var userEntity = await _userRepo.Get(x => x.UserName == model.UserName)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(true);

            // Check User is exist
            
            if (userEntity == null)
            {
                throw new GoblinException(nameof(GoblinIdentityErrorCode.UserNotFound), GoblinIdentityErrorCode.UserNotFound);
            }

            // Compare password hash from request and database

            var passwordHash = PasswordHelper.HashPassword(model.Password, userEntity.PasswordLastUpdatedTime);

            if (passwordHash != userEntity.PasswordHash)
            {
                throw new GoblinException(nameof(GoblinIdentityErrorCode.WrongPassword), GoblinIdentityErrorCode.WrongPassword);
            }
            
            // Generate Access Token

            var now = GoblinDateTimeHelper.SystemTimeNow;
            
            var accessTokenData = new TokenDataModel<AccessTokenDataModel>
            {
                ExpireTime = now.Add(SystemSetting.Current.AccessTokenLifetime),
                CreatedTime = now,
                Data =  new AccessTokenDataModel
                {
                    UserId = userEntity.Id
                }
            };

            var accessToken = JwtHelper.Generate(accessTokenData);

            return accessToken;
        }

        private void CheckUniqueUserName(string userName, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return;
            }

            var query = _userRepo.Get(x => x.UserName == userName);

            if (excludeId != null)
            {
                query = query.Where(x => x.Id != excludeId);
            }

            var isUnique = !query.Any();

            if (!isUnique)
            {
                throw new GoblinException(nameof(GoblinIdentityErrorCode.UserNameNotUnique), GoblinIdentityErrorCode.UserNameNotUnique);
            }
        }

        private void CheckUniqueEmail(string email, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            var query = _userRepo.Get(x => x.Email == email && x.EmailConfirmedTime != null);

            if (excludeId != null)
            {
                query = query.Where(x => x.Id != excludeId);
            }

            var isUnique = !query.Any();

            if (!isUnique)
            {
                throw new GoblinException(nameof(GoblinIdentityErrorCode.EmailNotUnique), GoblinIdentityErrorCode.EmailNotUnique);
            }
        }
    }
}