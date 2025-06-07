using FinanceApp.Models;
using SQLite;
using BCrypt.Net;

namespace FinanceApp.Services
{
    public class AppDatabase

    {
        private SQLiteAsyncConnection? db;
        const int CurrentDbVersion = 7;
        private const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create;
        async Task Init()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "appDatabase.db");
            int DbVersion = Preferences.Get("DbVersion", 0);

            if (DbVersion < CurrentDbVersion)
            {
                if (db is not null)
                {
                    await db.CloseAsync();
                    db = null;
                    File.Delete(databasePath);
                }
                Preferences.Set("DbVersion", CurrentDbVersion);
            }

            if (db is null)
            {
                db = new SQLiteAsyncConnection(databasePath, Flags);
                await db.CreateTableAsync<DataTransaction>();
                await db.CreateTableAsync<Users>();
                await db.CreateTableAsync<Epargne>();
            }
        }

        public async Task<int> AddEpargne(Epargne Data)
        {
            await Init();
            return await db.InsertAsync(Data);
        }
        public async Task<List<Epargne>> GetAllEpargnes()
        {
            await Init();
            return await db.Table<Epargne>().ToListAsync();
        }

        public async Task<int> DeleteEpargne(int Id)
        {
            await Init();
            return await db.DeleteAsync<Epargne>(Id);
        }
        public async Task<int> DeleteAllEpargne()
        {
            await Init();
            return await db.DeleteAllAsync<Epargne>();
        }
        public async Task<int> UpdateAllEpargnes(double somme)
        {
            await Init();
            var epargnes = await GetAllEpargnes();
            if( epargnes.Count == 1)
            {
                return await db.ExecuteAsync(" UPDATE Epargne SET MonatantCourant = MonatantCourant + ?", somme);
            }
            return await db.ExecuteAsync("UPDATE Epargne SET MonatantCourant = MonatantCourant + ((Pourcentage * 1.0) / 100.0) * ?", somme);

            double Excedants = 0;
            foreach(var item in epargnes)
            {
                if(item.MonatantCourant > item.MontantFinal)
                {
                    Excedants += item.MonatantCourant - item.MontantFinal;
                    await db.ExecuteAsync("UPDATE Epargne SET MonotantCourant = MontantFinal");
                }
            }

            //// Mise à des stats du compte de l'utilisateur. 
            //// await db.ExecuteAsync("UPDATE Epargne SET "); 
            Preferences.Set("solde", (float)Excedants);
        }

        ////// Add a transaction to the database

        public async Task<int> AddTransaction(DataTransaction Data)
        {
            await Init();
            return await db.InsertAsync(Data);
        }

        public async Task<List<DataTransaction>> GetAllTransactions()
        {
            await Init();
            return await db.Table<DataTransaction>().ToListAsync();
        }
        public async Task<int> DeleteTransaction(int Id)
        {
            await Init();
            return await db.DeleteAsync<DataTransaction>(Id);
        }
        public async Task<int> UpdateTransaction(DataTransaction Data)
        {
            await Init();
            return await db.UpdateAsync(Data);
        }
        public async Task<DataTransaction> GetTransaction(int id)
        {
            await Init();
            return await db.Table<DataTransaction>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Users>> GetAllUsersAsync() => await db.Table<Users>().ToListAsync();

        public async Task<int> InsertUserAsync(Users user)
        {
            await Init();
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                if (!string.IsNullOrEmpty(user.Password) && !user.Password.StartsWith("$2"))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                return await db.InsertAsync(user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de l'insertion de l'utilisateur : {ex.Message}");
                throw;
            }
        }

        //methode pour mettre à jour un utilisateur
        public async Task UpdateUserAsync(Users user)
        {
            try
            {
                if (user is null)
                {
                    throw new ArgumentNullException(nameof(user));
                }
                await db.UpdateAsync(user);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la mise à jour de l'utilisateur : {ex.Message}");

            }
        }

        //methode pour supprimer un utilisateur
        public async Task DeleteUserAsync(Users user)
        {
            await Init();
            await db.DeleteAsync(user);
        }

        public async Task<int> UpdateUser(Users user)
        {
            await Init();
            return await db.UpdateAsync(user);
        }

        //methode pour recuperer un utilisateur par son email
        public async Task<Users> GetUserByEmailAsync(string email)
        {
            await Init();
            return await db.Table<Users>().Where(u => u.Email == email).FirstOrDefaultAsync();
        }

        //methode pour valider un utilisateur en verifiant son email et son mot de passe
        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            await Init();
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return false;
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        // methode pour verifier si l'email est unique
        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            await Init();
            var user = await GetUserByEmailAsync(email);
            return user == null;
        }

        public async Task<Users> GetUserByEmailAndPasswordAsync(string email, string password)
        {
            await Init();
            try
            {
                var user = await db.Table<Users>()
                                          .Where(x => x.Email == email)
                                          .FirstOrDefaultAsync();

                // Vérification avec BCrypt
                if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
                    return user;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors de la récupération de l'utilisateur : {ex.Message}");
            }
            return null;
        }
    }
}
