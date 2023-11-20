using Microsoft.AspNetCore.Mvc;
using Todo.User.Api.Models;

namespace Todo.User.Api.Interfaces
{
    public interface IUserRepository
    {
        public IEnumerable<UserInfo> Get();
        public UserInfo Get(int id);
        public UserInfo Get(string login);
        public void Post(UserInfo user);
        public void Put(UserInfo value);
        public void Delete(int id);

    }
}
