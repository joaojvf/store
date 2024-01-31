using System.ComponentModel.DataAnnotations;

namespace Identity.Api.Models
{
    public class UserRegister
    {
        [Required(ErrorMessage = "The field {0} is obrigatory.")]
        [EmailAddress(ErrorMessage = "The field {0} must be valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is obrigatory.")]
        [StringLength(100, ErrorMessage = "The field {0} must be between {2} and {1} characters.", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The password need be equals.")]
        public string PasswordConfirmation { get; set; }
    }

    public class UserLogin
    {
        [Required(ErrorMessage = "The field {0} is obrigatory.")]
        [EmailAddress(ErrorMessage = "The field {0} must be valid")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The field {0} is obrigatory.")]
        [StringLength(100, ErrorMessage = "The field {0} must be between {2} and {1} characters.", MinimumLength = 6)]
        public string Password { get; set; }
    }

    public class UserLoginResponse
    {
        public string AccessToken { get; set; }
        public double ExpirateIn { get; set; }
        public UserToken UserToken { get; set; }
    }

    public class UserToken
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IEnumerable<UserClaim> Claims { get; set; }
    }

    public class UserClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
