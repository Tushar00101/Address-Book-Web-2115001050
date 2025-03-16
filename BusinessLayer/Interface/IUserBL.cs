using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelLayer.Model;
using RepositoryLayer.Entity;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        public UserEntity RegisterBL(UserDTO userDTO);
        ResponseLoginDTO LoginBL(LoginDTO loginDTO);

        bool ForgetBL(ForgetDTO forgetDTO);
        bool UpdateUserPassword(string email, ResetDTO resetDTO);

        UserEntity GetUserProfile(int userId);
    }
}
