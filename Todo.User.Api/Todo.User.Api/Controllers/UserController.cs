using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Todo.User.Api.Models;
using Todo.User.Api.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.User.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly IMapper mapper;

        public UserController(UserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<UserInfo> Get()
        {
            var users = userRepository.Get();
            return users;
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var user = userRepository.Get(id);
            return Ok(user);
        }

        // POST api/<UserController>
        [HttpPost("register")]
        public IActionResult Post([FromBody] UserDto userDto)
        {
            var user = mapper.Map<UserInfo>(userDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            userRepository.Post(user);

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login(UserDto userDto)
        {
            var user = userRepository.Get(userDto.Login);
            return Ok(user);
        }

        // PUT api/<UserController>/5
        [HttpPut]
        public IActionResult Put([FromBody] UserInfo user)
        {
            userRepository.Put(user);
            return Ok("Пользователь обновлен.");
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            userRepository.Delete(id);
            return Ok();
        }
    }
}
