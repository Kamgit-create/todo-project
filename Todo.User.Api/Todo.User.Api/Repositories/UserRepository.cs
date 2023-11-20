using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Todo.User.Api.Data;
using Todo.User.Api.Interfaces;
using Todo.User.Api.Models;

namespace Todo.User.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly string connectionString;

        public UserRepository(DataContext context)
        {
            this.context = context;
            connectionString = context.connectionString;
        }

        public void Delete(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            string deleteUserQuery = "DELETE FROM Users WHERE Id = @Id";
            connection.Execute(deleteUserQuery, new { Id = id });
        }

        public IEnumerable<UserInfo> Get()
        {
            using var connection = new SqliteConnection(connectionString);
            string selectAllQuery = "SELECT * FROM Users";
            return connection.QueryFirstOrDefault<IEnumerable<UserInfo>>(selectAllQuery);
        }

        public UserInfo Get(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string selectByIdQuery = "SELECT * FROM Users WHERE Id = @Id";
            return connection.QueryFirstOrDefault<UserInfo>(selectByIdQuery, new { Id = id });

        }

        public UserInfo Get(string login)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            string selectByIdQuery = "SELECT * FROM Users WHERE Login = @Login";
            return connection.QueryFirstOrDefault<UserInfo>(selectByIdQuery, new { Login = login });
        }

        public void Post(UserInfo user)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Execute(@"INSERT INTO Users (Login, Email, PasswordHash, RefreshToken, TokenCreated, TokenExpires) 
                             VALUES (@Login, @Email, @PasswordHash, @RefreshToken, @TokenCreated, @TokenExpires)", user);
        }

        public void Put(UserInfo user)
        {
            using var connection = new SqliteConnection(connectionString);
            string putQuery = @"UPDATE Users 
                            SET Login = @Login, 
                                Email = @Email, 
                                PasswordHash = @PasswordHash, 
                                RefreshToken = @RefreshToken, 
                                TokenCreated = @TokenCreated, 
                                TokenExpires = @TokenExpires 
                            WHERE Id = @Id";
            connection.Execute(putQuery, user);
        }
    }
}