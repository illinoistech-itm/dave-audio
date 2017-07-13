using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FinalProject
{
    public static class SqliteTransactions
    {
        public static void AddUser(User user)
        {
            using (var db = new UserContext())
            {
                db.Users.Add(user);
                try
                {
                    db.SaveChanges();
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException dbExc)
                {
                    Debug.WriteLine("Error: " + dbExc.InnerException.Message);
                }
            }
        }

        public static void ModifyAccessByUserId(String userId, bool access)
        {
            User userAux;
            using (var db = new UserContext())
            {
                userAux = (from user in db.Users
                             where user.userId.Equals(userId)
                             select user).FirstOrDefault();
                userAux.access = access;
            }
            using (var dbaux = new UserContext())
            {
                dbaux.Entry(userAux).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                dbaux.SaveChanges();
            }
        }

        public static List<User> GetAllUsers()
        {
            using (var db = new UserContext())
            {
                return db.Users.ToList();
            }
        }

        public static User GetUserByUserId(String userId)
        {
            using (var db = new UserContext())
            {
                return (from user in db.Users
                        where user.userId.Equals(userId)
                        select user).FirstOrDefault();
            }
        }

        public static String GetPhraseByUserId(String userId)
        {
            using (var db = new UserContext())
            {
                return (from user in db.Users
                        where user.userId.Equals(userId)
                        select user.phrase).FirstOrDefault();
            }
        }

        public static Boolean GetAccessByUserId(String userId)
        {
            using (var db = new UserContext())
            {
                return (from user in db.Users
                        where user.userId.Equals(userId)
                        select user.access).FirstOrDefault();
            }
        }

        public static void DeleteAllUsers()
        {
            using (var db = new UserContext())
            {
                foreach (var user in db.Users)
                {
                    db.Users.Remove(user);
                }

                db.SaveChanges();
            }
        }

        public static void DeleteUserbyUserId(String userId)
        {
            using (var db = new UserContext())
            {
                if ((from user in db.Users
                     where user.userId.Equals(userId)
                     select user).FirstOrDefault() != null)
                {
                    db.Users.Remove((from user in db.Users
                                     where user.userId.Equals(userId)
                                     select user).FirstOrDefault());
                    db.SaveChanges();
                }

            }
        }

        public static void ShowUsersDB()
        {
            Debug.WriteLine("These are the actual users in the system.");
            foreach (User user in SqliteTransactions.GetAllUsers())
            {
                Debug.WriteLine("UserID: " + user.userId + " First Name: " + user.firstName + " Last Name: " + user.lastName + " Phrase: " + user.phrase + " Speaker Id: " + user.speakerId);
            }
        }
    }
}
