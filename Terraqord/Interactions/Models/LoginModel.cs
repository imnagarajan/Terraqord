namespace Terraqord.Interactions.Models
{

    public class LoginModel : IModal
    {
        public string Title
            => "Fill the fields to log in:";

        [InputLabel("Username:")]
        [RequiredInput(true)]
        [ModalTextInput("entry1", TextInputStyle.Short, "Frank")]
        public string Username { get; set; } = string.Empty;

        [InputLabel("Password:")]
        [RequiredInput(true)]
        [ModalTextInput("entry2", TextInputStyle.Short, "MyCoolPassword")]
        public string Password { get; set; } = string.Empty;
    }
}
