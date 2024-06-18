using System;

namespace LegacyApp
{
   
        
        
        public interface ICreditLimitService
        {
            int GetCreditLimit(string lastName, DateTime birthdate);
        }
        
        public interface IClientRepository
        {
            Client GetById(int IdClient);
        }
        
    public class UserService
    {
        private ClientRepository _clientRepository;
        private UserCreditService _creditService;

        
        [Obsolete]
        public UserService()
        {
            _clientRepository = new ClientRepository();
            _creditService = new UserCreditService();
        }
        
        public UserService(ClientRepository clientRepository,
            UserCreditService creditService)
        {
            _clientRepository = clientRepository;
            _creditService = creditService;
        }
        
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            bool validateEmail(string email)
            {
                if (!email.Contains("@") && !email.Contains(".")) return false;
                return true;
            }

            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            
            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {
                int creditLimit = _creditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                creditLimit = creditLimit * 2;
                user.CreditLimit = creditLimit;
            }
            else
            {
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
