using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        Entity.UserEntity RegisterRL(UserDTO userDTO);
        UserEntity LoginRL(LoginDTO loginDTO);
        bool ForgetRL(ForgetDTO forgetDTO);

        bool UpdateUserPassword(string email, ResetDTO resetDTO);

        UserEntity GetUserProfile(int userId);
    }
}
