using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SaladDemo {
  public class UserRepository {
    private List<UserEntry> Users = new List<UserEntry>();

    public UserEntry AddUser() {
      var newUser = new UserEntry();
      int UserID = 1;
      if(Users.Any()) {
        UserID = Users.Max(u => u.userId) + 1;
      }
      newUser.userId = UserID;
      Users.Add(newUser);
      return newUser;
    }

    public UserEntry GetUser(int userId) {
      var user = Users.Where(u => u.userId == userId).FirstOrDefault();
      return user;
    }
  }
}
