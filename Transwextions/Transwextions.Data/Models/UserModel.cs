using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transwextions.Data.Models;

public class UserModel
{
    public string Username { get; set; } = string.Empty;
    public string AvatarImagePath { get; set; } = "./images/avatar/placeholder.webp";
}