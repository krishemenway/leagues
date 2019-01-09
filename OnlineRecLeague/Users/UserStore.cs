﻿using Dapper;
using OnlineRecLeague.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineRecLeague.Users
{
	public interface IUserStore
	{
		bool TryFindUserById(Guid userId, out IUser user);
		bool TryFindUserByEmail(string emailAddress, out IUser user);

		IReadOnlyList<IUser> FindUsers(IReadOnlyList<Guid> userIds);
		IReadOnlyList<IUser> FindUsersByQuery(string query);

		IUser CreateNewUser(CreateNewUserRequest request);
		void EmailConfirmed(IUser user, DateTimeOffset emailConfirmationTime);
	}

	internal class UserStore : IUserStore
	{
		public UserStore(IRegionStore regionStore = null)
		{
			_regionStore = regionStore ?? new RegionStore();
		}

		public bool TryFindUserById(Guid userId, out IUser user)
		{
			user = FindUsers(new[] { userId }).FirstOrDefault();
			return user != null;
		}

		public bool TryFindUserByEmail(string email, out IUser user)
		{
			if (email == SuperAdmin.Email)
			{
				user = SuperAdmin;
				return true;
			}

			const string sql = @"
				SELECT
					user_id as userid,
					nickname,
					realname,
					email,
					password_hash as passwordhash,
					email_confirmation_code as emailconfirmationcode,
					email_confirmation_time as emailconfirmationtime,
					join_time as jointime,
					region,
					default_timezone as defaulttimezone
				FROM
					svc.user
				WHERE
					email = @Email";

			using (var connection = Database.CreateConnection())
			{
				user = connection.Query<UserRecord>(sql, new { email }).Select(Create).SingleOrDefault();
				return user != null;
			}
		}

		public IReadOnlyList<IUser> FindUsers(IReadOnlyList<Guid> userIds)
		{
			const string sql = @"
				SELECT
					user_id as userid,
					nickname,
					realname,
					email,
					password_hash as passwordhash,
					email_confirmation_code as emailconfirmationcode,
					email_confirmation_time as emailconfirmationtime,
					join_time as jointime,
					region,
					default_timezone as defaulttimezone
				FROM
					svc.user
				WHERE
					user_id = any(@UserIds)";

			using (var connection = Database.CreateConnection())
			{
				return connection.Query<UserRecord>(sql, new { userIds }).Select(Create).ToList();
			}
		}

		public IReadOnlyList<IUser> FindUsersByQuery(string query)
		{
			const string sql = @"
				SELECT
					user_id as userid,
					nickname,
					realname,
					email,
					password_hash as passwordhash,
					email_confirmation_code as emailconfirmationcode,
					email_confirmation_time as emailconfirmationtime,
					join_time as jointime,
					region,
					default_timezone as defaulttimezone
				FROM
					svc.user
				WHERE
					email like @Query OR nickname like @Query";

			using (var connection = Database.CreateConnection())
			{
				return connection.Query<UserRecord>(sql, new { Query = $"%{query}%" }).Select(Create).ToList();
			}
		}

		public IUser CreateNewUser(CreateNewUserRequest request)
		{
			const string sql = @"
				INSERT INTO svc.user
				(nickname, realname, email, password_hash, join_time, default_timezone, region, email_confirmation_code, email_confirmation_time)
				VALUES
				(@NickName, @RealName, @Email, @PasswordHash, @JoinTime, @DefaultTimezone, @Region, @EmailConfirmationCode, null)
				RETURNING user_id;";

			using (var connection = Database.CreateConnection())
			{
				var sqlParams = new
					{
						request.NickName,
						request.RealName,
						request.Email,
						PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
						request.JoinTime,
						request.DefaultTimezone,
						request.Region,
						EmailConfirmationCode = Guid.NewGuid(),
					};

				var userId = connection.QuerySingle<Guid>(sql, sqlParams);
				return FindUsers(new[] { userId }).Single();
			}
		}

		public void EmailConfirmed(IUser user, DateTimeOffset emailConfirmationTime)
		{
			const string sql = @"
				UPDATE svc.user
				SET email_confirmation_time = @EmailConfirmationTime
				WHERE user_id = @UserId;";

			using (var connection = Database.CreateConnection())
			{
				connection.Execute(sql, new { user.UserId, emailConfirmationTime });
			}
		}

		private IUser Create(UserRecord userRecord)
		{
			return new User(userRecord.UserId)
				{
					NickName = userRecord.NickName,
					RealName = userRecord.RealName,

					Email = userRecord.Email,
					PasswordHash = userRecord.PasswordHash,

					EmailConfirmationCode = userRecord.EmailConfirmationCode,
					EmailConfirmedTime = userRecord.EmailConfirmationTime,

					JoinTime = userRecord.JoinTime,
					QuitTime = userRecord.QuitTime,

					IsSuperAdmin = false,

					Region = _regionStore.FindRegionOrThrow(userRecord.Region),
					DefaultTimezone = TimeZoneInfo.FindSystemTimeZoneById(userRecord.DefaultTimezone)
				};
		}

		private static IUser SuperAdmin => new User(Guid.Empty) { NickName = "SuperAdmin", Email = "superadmin", IsSuperAdmin = true, PasswordHash = BCrypt.Net.BCrypt.HashPassword(Program.Settings.SuperadminPassword) };

		private readonly IRegionStore _regionStore;
	}
}